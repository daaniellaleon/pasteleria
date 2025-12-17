const db = require("../db/db");
const PDFDocument = require("pdfkit");

// =======================================================
// UTILIDADES
// =======================================================
function num(v) {
    return Number(v) || 0;
}

function fechaElegante(fecha) {
    const d = new Date(fecha);
    return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,"0")}-${String(d.getDate()).padStart(2,"0")}`;
}

function resolverRango(query) {
    const { fecha, inicio, fin } = query;
    if (fecha) return { ini: fecha, fin: fecha };
    if (inicio && fin) return { ini: inicio, fin: fin };
    return null;
}

const reporteInventario = async (req, res) => {
    try {
        const rango = resolverRango(req.query);
        const formato = req.query.formato || "pdf"; // pdf, csv, excel

        if (!rango)
            return res.status(400).json({ error: "Debe enviar fecha o rango válido." });

        // =======================
        // Obtener Productos
        // =======================
        const [productos] = await db.query(
            `SELECT nombre, categoria_id, precio_unitario, stock, estado, fecha_ingreso, fecha_modificacion
             FROM productos 
             WHERE fecha_ingreso BETWEEN ? AND ?
             ORDER BY fecha_ingreso ASC`,
            [rango.ini, rango.fin]
        );

        // =======================
        // Obtener Insumos
        // =======================
        const [insumos] = await db.query(
            `SELECT insumo, cantidad, unidad_medida, proveedor, estado, fecha_ingreso, fecha_modificacion
             FROM insumos
             WHERE fecha_ingreso BETWEEN ? AND ?
             ORDER BY fecha_ingreso ASC`,
            [rango.ini, rango.fin]
        );

        if (!productos.length && !insumos.length)
            return res.status(404).json({ message: "No hay datos de inventario en ese rango" });

        // =======================
        // CSV
        // =======================
        if (formato === "csv") {
            let csv = "TIPO,NOMBRE/CODIGO,CANTIDAD/ STOCK,UNIDAD/PRECIO,ESTADO,FECHA\n";

            productos.forEach(p => {
                csv += `PRODUCTO,${p.nombre},${p.stock},$${p.precio_unitario},${p.estado},${fechaElegante(p.fecha_ingreso)}\n`;
            });

            insumos.forEach(i => {
                csv += `INSUMO,${i.insumo},${i.cantidad},${i.unidad_medida},${i.estado},${fechaElegante(i.fecha_ingreso)}\n`;
            });

            res.setHeader("Content-Type", "text/csv");
            res.setHeader("Content-Disposition", "attachment; filename=reporte_inventario.csv");
            return res.send(csv);
        }

        // =======================
        // EXCEL
        // =======================
        if (formato === "excel") {
            const workbook = new ExcelJS.Workbook();
            const sheet = workbook.addWorksheet("Inventario");

            sheet.addRow(["TIPO", "NOMBRE", "CANTIDAD / STOCK", "UNIDAD / PRECIO", "ESTADO", "FECHA"]);

            sheet.getRow(1).font = { bold: true };

            productos.forEach(p => {
                sheet.addRow([
                    "Producto",
                    p.nombre,
                    p.stock,
                    `$${p.precio_unitario}`,
                    p.estado,
                    fechaElegante(p.fecha_ingreso)
                ]);
            });

            insumos.forEach(i => {
                sheet.addRow([
                    "Insumo",
                    i.insumo,
                    i.cantidad,
                    i.unidad_medida,
                    i.estado,
                    fechaElegante(i.fecha_ingreso)
                ]);
            });

            sheet.columns.forEach(col => col.width = 20);

            res.setHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            res.setHeader("Content-Disposition", "attachment; filename=reporte_inventario.xlsx");

            await workbook.xlsx.write(res);
            return res.end();
        }

        // =======================
        // PDF
        // =======================
        const doc = new PDFDocument({ margin: 40 });
        res.setHeader("Content-Type", "application/pdf");
        doc.pipe(res);

        encabezado(
            doc,
            "Reporte de Inventario",
            rango.ini === rango.fin
                ? `Fecha: ${rango.ini}`
                : `Periodo: ${rango.ini} a ${rango.fin}`
        );

        // Sección productos
        // Sección productos
if (productos.length) {
    doc.fontSize(16).fillColor("#0F5E3A").text("Productos", { underline: true });
    doc.moveDown(0.5);

    productos.forEach(p => {
        doc.fontSize(12).fillColor("#000");
        doc.text(`Nombre: ${p.nombre}`);
        doc.text(`Stock: ${p.stock}`);
        doc.text(`Precio Unitario: $${p.precio_unitario}`);
        doc.text(`Estado: ${p.estado}`);
        doc.text(`Fecha Ingreso: ${fechaElegante(p.fecha_ingreso)}`);
        doc.moveDown(1);
    });
}

        // Sección insumos
        if (insumos.length) {
            doc.addPage();
            doc.fontSize(16).fillColor("#0F5E3A").text("Insumos", { underline: true });
            doc.moveDown(0.5);

            insumos.forEach(i => {
                doc.fontSize(12).fillColor("#000");
                doc.text(`Insumo: ${i.insumo}`);
                doc.text(`Cantidad: ${i.cantidad} ${i.unidad_medida}`);
                doc.text(`Proveedor: ${i.proveedor}`);
                doc.text(`Estado: ${i.estado}`);
                doc.text(`Fecha Ingreso: ${fechaElegante(i.fecha_ingreso)}`);
                doc.moveDown(1);
            });
        }

        doc.end();

    } catch (err) {
        console.error(err);
        return res.status(500).json({ error: "Error generando reporte", detalle: err });
    }
};





























// =======================================================
// ENCABEZADO PROFESIONAL
// =======================================================
function encabezado(doc, titulo, sub) {
    const verde = "#0F5E3A";

    doc.save();
    doc.rect(0, 0, doc.page.width, 85).fill(verde);
    doc.fillColor("#FFF");
    doc.fontSize(26).text(titulo, 0, 20, { align: "center" });
    doc.fontSize(12).text(sub, 0, 55, { align: "center" });
    doc.restore();

    doc.moveDown(3);
}

// =======================================================
// TABLA ESTILO EXCEL (FLUIDA Y PROFESIONAL)
// =======================================================
function tablaVentas(doc, rows) {

    // CONFIGURACIÓN DE COLUMNAS
    const colWidths = [140, 160, 140, 120];
    const headers = ["TRANSACCIÓN", "MÉTODO DE PAGO", "FECHA", "TOTAL"];
    const rowHeight = 22;

    // ANCHO TOTAL
    const tableWidth = colWidths.reduce((a, b) => a + b, 0);

    // POSICIÓN CENTRADA
    const pageWidth = doc.page.width;
    const startX = (pageWidth - tableWidth) / 2;

    // --- TÍTULO DE TABLA ---
    doc.fontSize(16)
        .fillColor("#0F5E3A")
        .text("Tabla de Ventas", startX, doc.y, { underline: true });

    // Espacio controlado
    doc.moveDown(0.8);

    // --- HEADER EXACTAMENTE ALINEADO ---
    let headerY = doc.y;

    doc.fontSize(12).fillColor("#0F5E3A");
    let x = startX;

    headers.forEach((h, i) => {
        doc.text(h, x, headerY, {
            width: colWidths[i],
            align: "center"
        });
        x += colWidths[i];
    });

    // LÍNEA BAJO HEADER (ALINEADA EXACTA)
    const lineY = headerY + 18;

    doc.moveTo(startX, lineY)
       .lineTo(startX + tableWidth, lineY)
       .stroke();

    // ACTUALIZAR CURSOR CORRECTAMENTE
    doc.y = lineY + 6;
    doc.fontSize(11).fillColor("#000");

    // --- FILAS ---
    rows.forEach((r) => {
        const y = doc.y;

        const rowData = [
            r.transaction_id,
            r.metodo_pago,
            fechaElegante(r.fecha),
            `$${num(r.total).toFixed(2)}`
        ];

        let colX = startX;

        rowData.forEach((text, i) => {
            doc.text(text, colX, y, {
                width: colWidths[i],
                align: "center"
            });
            colX += colWidths[i];
        });

        // LÍNEA INFERIOR DE FILA
        doc.moveTo(startX, y + rowHeight)
           .lineTo(startX + tableWidth, y + rowHeight)
           .stroke();

        doc.y = y + rowHeight + 5;

        if (doc.y > 720) {
            doc.addPage();
            doc.y = 100;
        }
    });

    doc.moveDown(2);
}
// =======================================================
// CONTROLADOR FINAL 
// =======================================================
const reporteVentas = async (req, res) => {
    try {
        const rango = resolverRango(req.query);
        const formato = req.query.formato || "pdf"; // pdf, csv, excel

        if (!rango)
            return res.status(400).json({ error: "Debe enviar fecha o rango válido." });

        const [rows] = await db.query(
            `SELECT fecha, total, metodo_pago, transaction_id
             FROM compras 
             WHERE fecha BETWEEN ? AND ?
             ORDER BY fecha ASC`,
            [rango.ini, rango.fin]
        );

        if (!rows.length)
            return res.status(404).json({ message: "Sin ventas en ese periodo" });

        // 📌 FORMATO CSV
        if (formato === "csv") {
            const csv = generarCSV(rows);
            res.setHeader("Content-Type", "text/csv");
            res.setHeader("Content-Disposition", "attachment; filename=reporte_ventas.csv");
            return res.send(csv);
        }

        // 📌 FORMATO EXCEL
        if (formato === "excel") {
            return generarExcel(rows, res);
        }

        // 📌 FORMATO PDF (actual)
        const doc = new PDFDocument({ margin: 40 });
        res.setHeader("Content-Type", "application/pdf");
        doc.pipe(res);

        encabezado(
            doc,
            "Reporte de Ventas",
            rango.ini === rango.fin
                ? `Fecha: ${rango.ini}`
                : `Periodo: ${rango.ini} a ${rango.fin}`
        );

        tablaVentas(doc, rows);
        resumenGeneral(doc, rows);

        doc.end();

    } catch (err) {
        console.error(err);
        return res.status(500).json({
            error: "Error generando reporte",
            detalle: err
        });
    }
};


// =======================================================
// GeNERAR RESUMEN GENERAL
// =======================================================

function resumenGeneral(doc, rows) {
    // Valores
    const ventas = rows.map(r => Number(r.total));
    const total = ventas.reduce((a, b) => a + b, 0);
    const prom = ventas.length ? total / ventas.length : 0;
    const min = ventas.length ? Math.min(...ventas) : 0;
    const max = ventas.length ? Math.max(...ventas) : 0;

    // Título
    doc.fontSize(16)
        .fillColor("#0F5E3A")
        .text("Resumen General", 60, doc.y, { underline: true });

    doc.moveDown(1);

    // Cuerpo del resumen (alineado a la izquierda)
    doc.fontSize(12).fillColor("#000");

    doc.text(`Total vendido: $${total.toFixed(2)}`, 60);
    doc.text(`Promedio por venta: $${prom.toFixed(2)}`, 60);
    doc.text(`Venta mínima: $${min.toFixed(2)}`, 60);
    doc.text(`Venta máxima: $${max.toFixed(2)}`, 60);

    doc.moveDown(2);
}



// =======================================================
// GENERAR CSV SIMPLE
// =======================================================

function generarCSV(rows) {
    let csv = "transaction_id,metodo_pago,fecha,total\n";

    rows.forEach(r => {
        csv += `${r.transaction_id},${r.metodo_pago},${fechaElegante(r.fecha)},${r.total}\n`;
    });

    return csv;
}

// =======================================================
// GENERAR EXCEL PROFESIONAL
// =======================================================
const ExcelJS = require("exceljs");

async function generarExcel(rows, res) {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet("Reporte de Ventas");

    // Encabezados
    sheet.addRow(["Transacción", "Método de Pago", "Fecha", "Total"]);

    // Estilo del header
    sheet.getRow(1).font = { bold: true, color: { argb: "FFFFFFFF" } };
    sheet.getRow(1).fill = {
        type: "pattern",
        pattern: "solid",
        fgColor: { argb: "FF0F5E3A" }
    };

    // Agregar filas
    rows.forEach(r => {
        sheet.addRow([
            r.transaction_id,
            r.metodo_pago,
            fechaElegante(r.fecha),
            Number(r.total)
        ]);
    });

    // Auto-size
    sheet.columns.forEach(col => {
        col.width = 20;
    });

    // Enviar Excel
    res.setHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    res.setHeader("Content-Disposition", "attachment; filename=reporte_ventas.xlsx");

    await workbook.xlsx.write(res);
    res.end();
}

module.exports = {
    reporteVentas,
    reporteInventario
};
