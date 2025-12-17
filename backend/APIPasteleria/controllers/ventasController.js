const pool = require("../db/db");

// Agregar producto al carrito
async function agregarAlCarrito(req, res) {
  const usuarioId = req.user.id;
  const { nombre, cantidad } = req.body;

  // Validar cantidad
  if (!Number.isInteger(cantidad) || cantidad <= 0) {
    return res.status(400).json({ mensaje: "Cantidad debe ser un número entero mayor a 0" });
  }

  // Buscar producto por nombre (autocomplete backend)
  const [productos] = await pool.query(
    "SELECT * FROM productos WHERE nombre LIKE ? LIMIT 10",
    [`${nombre}%`]
  );

  if (!productos.length) {
    return res.status(404).json({ mensaje: "Producto no encontrado" });
  }

  const producto = productos[0];

  // Verificar stock
  if (cantidad > producto.stock) {
    return res.status(400).json({ 
      mensaje: `Cantidad solicitada (${cantidad}) excede el stock disponible (${producto.stock})` 
    });
  }

  // Verificar si el producto ya está en el carrito
  const [carritoExistente] = await pool.query(
    "SELECT * FROM carrito WHERE usuario_id = ? AND producto_id = ?",
    [usuarioId, producto.id]
  );

  if (carritoExistente.length > 0) {
    // Actualizar cantidad y total
    const nuevoStock = carritoExistente[0].cantidad + cantidad;
    if (nuevoStock > producto.stock) {
      return res.status(400).json({
        mensaje: `Cantidad total en carrito (${nuevoStock}) excede el stock disponible (${producto.stock})`
      });
    }

    const nuevoTotal = nuevoStock * producto.precio_unitario;
    await pool.query(
      "UPDATE carrito SET cantidad = ?, total = ? WHERE id = ?",
      [nuevoStock, nuevoTotal, carritoExistente[0].id]
    );

    return res.json({ 
      mensaje: "Cantidad del producto actualizada en el carrito", 
      producto: { nombre: producto.nombre, cantidad: nuevoStock, total: nuevoTotal }
    });
  }

  // Insertar nuevo producto en carrito
  const total = cantidad * producto.precio_unitario;
  await pool.query(
    "INSERT INTO carrito (producto_id, nombre, cantidad, precio_unitario, total, usuario_id) VALUES (?, ?, ?, ?, ?, ?)",
    [producto.id, producto.nombre, cantidad, producto.precio_unitario, total, usuarioId]
  );

  res.json({ mensaje: "Producto agregado al carrito", producto: { nombre: producto.nombre, cantidad, total } });
}

// Listar carrito
async function listarCarrito(req, res) {
  const usuarioId = req.user.id;
  try {
    const [carrito] = await pool.query(
      "SELECT * FROM carrito WHERE usuario_id = ?",
      [usuarioId]
    );
    res.json(carrito);
  } catch (err) {
    res.status(500).json({ mensaje: "Error listando carrito", error: err });
  }
}

// Eliminar producto del carrito
async function eliminarDelCarrito(req, res) {
  const { id } = req.params;
  try {
    const [result] = await pool.query("DELETE FROM carrito WHERE id = ?", [id]);
    if (result.affectedRows === 0) {
      return res.status(404).json({ mensaje: "Producto no encontrado en carrito" });
    }
    res.json({ mensaje: "Producto eliminado del carrito" });
  } catch (err) {
    res.status(500).json({ mensaje: "Error eliminando producto", error: err });
  }
}



// Actualizar cantidad de producto en el carrito
async function actualizarCantidad(req, res) {
  const { id } = req.params; // id del carrito
  const usuarioId = req.user.id;
  const { cantidad } = req.body;

  if (!Number.isInteger(cantidad) || cantidad <= 0) {
    return res.status(400).json({ mensaje: "Cantidad debe ser un entero mayor a 0" });
  }

  // Buscar producto en carrito
  const [carrito] = await pool.query(
    "SELECT * FROM carrito WHERE id = ? AND usuario_id = ?",
    [id, usuarioId]
  );

  if (carrito.length === 0) {
    return res.status(404).json({ mensaje: "Producto no encontrado en el carrito" });
  }

  const item = carrito[0];

  // Traer stock del producto real
  const [productos] = await pool.query(
    "SELECT * FROM productos WHERE id = ?",
    [item.producto_id]
  );

  if (productos.length === 0) {
    return res.status(404).json({ mensaje: "Producto no encontrado" });
  }

  const producto = productos[0];

  if (cantidad > producto.stock) {
    return res.status(400).json({ mensaje: `Cantidad (${cantidad}) excede stock disponible (${producto.stock})` });
  }

  const total = cantidad * item.precio_unitario;

  await pool.query(
    "UPDATE carrito SET cantidad = ?, total = ? WHERE id = ?",
    [cantidad, total, id]
  );

  res.json({ mensaje: "Cantidad actualizada", producto: { nombre: item.nombre, cantidad, total } });
}

module.exports = { agregarAlCarrito, listarCarrito, eliminarDelCarrito, actualizarCantidad };