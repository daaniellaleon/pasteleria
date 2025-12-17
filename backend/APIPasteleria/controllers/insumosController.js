const pool = require("../db/db");
const validarInsumo = require("../utils/insumosValidator");

// Listar insumos
async function listarInsumos(req, res) {
  try {
    const [rows] = await pool.query("SELECT * FROM insumos");
    res.json(rows);
  } catch (err) {
    res.status(500).json({ mensaje: "Error listando insumos", error: err });
  }
}

// Crear insumo
async function crearInsumo(req, res) {
  const { insumo, cantidad, unidad_medida, proveedor } = req.body;

const error = validarInsumo(req.body);
  if (error) return res.status(400).json({ mensaje: error });

  const estado = cantidad > 0 ? "disponible" : "agotado";
  try {
    await pool.query(
      "INSERT INTO insumos (insumo, cantidad, unidad_medida, proveedor, estado) VALUES (?, ?, ?, ?, ?)",
      [insumo, cantidad, unidad_medida, proveedor, estado]
    );
    res.json({ mensaje: "Insumo creado con éxito", estado });
  } catch (err) {
    res.status(500).json({ mensaje: "Error creando insumo", error: err });
  }
}

// Editar insumo
async function editarInsumo(req, res) {
  const { id } = req.params;
  const { insumo, cantidad, unidad_medida, proveedor } = req.body;


  const error = validarInsumo(req.body);
  if (error) return res.status(400).json({ mensaje: error });

  const estado = cantidad > 0 ? "disponible" : "agotado";

  try {
    const [result] = await pool.query(
      "UPDATE insumos SET insumo = ?, cantidad = ?, unidad_medida = ?, proveedor = ?, estado = ?, fecha_modificacion = NOW() WHERE id = ?",
      [insumo, cantidad, unidad_medida, proveedor, estado, id]
    );

    if (result.affectedRows === 0) {
      return res.status(404).json({ mensaje: "Insumo no encontrado" });
    }

    res.json({ mensaje: "Insumo actualizado con éxito", estado });
  } catch (err) {
    res.status(500).json({ mensaje: "Error actualizando insumo", error: err });
  }
}

module.exports = { listarInsumos, crearInsumo, editarInsumo };
