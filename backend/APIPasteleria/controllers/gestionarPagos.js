// controllers/ventasController.js
const pool = require("../db/db");

// -----------------------------------------------------------
// 1️⃣ Seleccionar método de pago (primer paso)
// -----------------------------------------------------------
async function seleccionarMetodo(req, res) {
  const usuarioId = req.user.id;
  const metodoPago = req.body.metodoPago?.toLowerCase();

  const metodosValidos = ["visa", "ach", "yappy", "efectivo"];
  if (!metodoPago || !metodosValidos.includes(metodoPago.toLowerCase())) {
    return res.status(400).json({ mensaje: "Método de pago no válido" });
  }

  if (metodoPago.toLowerCase() === "efectivo") {
    return res.json({
      mensaje: "Método efectivo seleccionado, falta monto recibido",
      requiereMonto: true
    });
  }

  // Si no es efectivo → pagar directo
  return pagarConMetodoDirecto(req, res, metodoPago);
}

// -----------------------------------------------------------
// 2️⃣ Pagar efectivo (segundo paso)
// -----------------------------------------------------------
async function pagarEfectivo(req, res) {
  const usuarioId = req.user.id;
  const { montoRecibido } = req.body;

  if (!montoRecibido || isNaN(montoRecibido) || montoRecibido <= 0) {
    return res.status(400).json({ mensaje: "Monto recibido inválido" });
  }

  try {
    const [carrito] = await pool.query(
      "SELECT * FROM carrito WHERE usuario_id = ?",
      [usuarioId]
    );

    if (carrito.length === 0) {
      return res.status(400).json({ mensaje: "El carrito está vacío" });
    }

    const total = carrito.reduce((sum, item) => sum + parseFloat(item.total), 0);

    if (montoRecibido < total) {
      return res.status(400).json({
        mensaje: "El monto recibido es menor que el total"
      });
    }

    const cambio = montoRecibido - total;
    const transactionId = Math.floor(Math.random() * 1000000).toString();

    const [compraResult] = await pool.query(
    `INSERT INTO compras 
      (usuario_id, fecha, total, metodo_pago, status, transaction_id, monto_recibido, cambio)
    VALUES (?, NOW(), ?, 'efectivo', 'success', ?, ?, ?)`,
    [usuarioId, total, transactionId, montoRecibido, cambio]
  );


    const compraId = compraResult.insertId;

    // Insertar detalle + actualizar stock
    for (const item of carrito) {
      await pool.query(
        "INSERT INTO detalle_compra (compra_id, producto_id, nombre, cantidad, precio_unitario, total) VALUES (?, ?, ?, ?, ?, ?)",
        [compraId, item.producto_id, item.nombre, item.cantidad, item.precio_unitario, item.total]
      );

            // 1️⃣ Restar stock
      await pool.query("UPDATE productos SET stock = stock - ? WHERE id = ?", [item.cantidad, item.producto_id]);

      // 2️⃣ Actualizar estado según nuevo stock
      await pool.query(
        "UPDATE productos SET estado = CASE WHEN stock <= 0 THEN 'agotado' ELSE 'disponible' END WHERE id = ?",
        [item.producto_id]
      );

    }

    await pool.query("DELETE FROM carrito WHERE usuario_id = ?", [usuarioId]);

    res.json({
      mensaje: "Pago en efectivo confirmado",
      compra: {
        id: compraId,
        total,
        montoRecibido,
        cambio,
        metodo_pago: "efectivo",
        status: "success",
        transactionId
      }
    });

  } catch (err) {
    console.error(err);
    return res.status(500).json({ mensaje: "Error procesando pago en efectivo" });
  }
}

// -----------------------------------------------------------
// 3️⃣ Pago con métodos digitales
// -----------------------------------------------------------
async function pagarConMetodoDirecto(req, res, metodoPago) {
  const usuarioId = req.user.id;

  try {
    const [carrito] = await pool.query(
      "SELECT * FROM carrito WHERE usuario_id = ?",
      [usuarioId]
    );

    if (carrito.length === 0) {
      return res.status(400).json({ mensaje: "El carrito está vacío" });
    }

    const total = carrito.reduce((sum, item) => sum + parseFloat(item.total), 0);
    const transactionId = Math.floor(Math.random() * 1000000).toString();

    const [compraResult] = await pool.query(
      "INSERT INTO compras (usuario_id, fecha, total, metodo_pago, status, transaction_id) VALUES (?, NOW(), ?, ?, 'success', ?)",
      [usuarioId, total, metodoPago, transactionId]
    );

    const compraId = compraResult.insertId;

    for (const item of carrito) {
      await pool.query(
        "INSERT INTO detalle_compra (compra_id, producto_id, nombre, cantidad, precio_unitario, total) VALUES (?, ?, ?, ?, ?, ?)",
        [compraId, item.producto_id, item.nombre, item.cantidad, item.precio_unitario, item.total]
      );

    // 1️⃣ Restar stock
    await pool.query("UPDATE productos SET stock = stock - ? WHERE id = ?", [item.cantidad, item.producto_id]);

    // 2️⃣ Actualizar estado según nuevo stock
    await pool.query(
      "UPDATE productos SET estado = CASE WHEN stock <= 0 THEN 'agotado' ELSE 'disponible' END WHERE id = ?",
      [item.producto_id]
    );

    }

    await pool.query("DELETE FROM carrito WHERE usuario_id = ?", [usuarioId]);

    res.json({
      mensaje: "Pago realizado con éxito",
      compra: {
        id: compraId,
        total,
        metodo_pago: metodoPago,
        status: "success",
        transactionId
      }
    });

  } catch (err) {
    console.error(err);
    return res.status(500).json({ mensaje: "Error procesando el pago" });
  }
}

// -----------------------------------------------------------
// EXPORTAR TODO
// -----------------------------------------------------------
module.exports = {
  seleccionarMetodo,
  pagarEfectivo,
  pagarConMetodoDirecto
};
