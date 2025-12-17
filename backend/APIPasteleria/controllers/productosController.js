const pool = require("../db/db");
const validarProducto = require("../utils/productoValidator");

// Listar productos
async function listarProductos(req, res) {
  try {
    const [rows] = await pool.query(
  `SELECT p.*, c.nombre as categoria 
   FROM productos p 
   JOIN categorias c ON p.categoria_id = c.id
   ORDER BY c.nombre ASC, p.nombre ASC`
);

    res.json(rows);
  } catch (err) {
    res.status(500).json({ mensaje: "Error listando productos", error: err });
  }
}

// Crear producto
async function crearProducto(req, res) {
  const { nombre, categoria, precio_unitario, stock } = req.body;

  const error = validarProducto(req.body);
  if (error) return res.status(400).json({ mensaje: error });

  try {
    // Buscar o crear categoría
    let [rows] = await pool.query("SELECT id FROM categorias WHERE nombre = ?", [categoria]);
    let categoria_id;
    if (rows.length > 0) {
      categoria_id = rows[0].id;
    } else {
      const [result] = await pool.query("INSERT INTO categorias (nombre) VALUES (?)", [categoria]);
      categoria_id = result.insertId;
    }

    const estado = stock > 0 ? "disponible" : "agotado";

    const [producto] = await pool.query(
      "INSERT INTO productos (nombre, categoria_id, precio_unitario, stock, estado) VALUES (?, ?, ?, ?, ?)",
      [nombre, categoria_id, precio_unitario, stock, estado]
    );

    res.json({ mensaje: "Producto creado", productoId: producto.insertId, estado });
  } catch (err) {
    res.status(500).json({ mensaje: "Error creando producto", error: err });
  }
}

// Editar producto
async function editarProducto(req, res) {
  const { id } = req.params;
  const { nombre, categoria, precio_unitario, stock } = req.body;

  const error = validarProducto(req.body);
  if (error) return res.status(400).json({ mensaje: error });

  try {
    // Buscar o crear categoría
    let [rows] = await pool.query("SELECT id FROM categorias WHERE nombre = ?", [categoria]);
    let categoria_id;
    if (rows.length > 0) {
      categoria_id = rows[0].id;
    } else {
      const [result] = await pool.query("INSERT INTO categorias (nombre) VALUES (?)", [categoria]);
      categoria_id = result.insertId;
    }

    const stockNumber = parseInt(stock);
    const estado = stockNumber > 0 ? "disponible" : "agotado";

    const nombreClean = nombre.trim();
    const categoriaClean = categoria.trim();

  const [result] = await pool.query(
    "UPDATE productos SET nombre = ?, categoria_id = ?, precio_unitario = ?, stock = ?, estado = ? WHERE id = ?",
    [nombreClean, categoria_id, precio_unitario, stockNumber, estado, id]
  );



    if (result.affectedRows === 0) {
      return res.status(404).json({ mensaje: "Producto no encontrado" });
    }

    res.json({ mensaje: "Producto actualizado con éxito", estado });
  } catch (err) {
    res.status(500).json({ mensaje: "Error actualizando producto", error: err });
  }
}

module.exports = { listarProductos, crearProducto, editarProducto };