const express = require("express");
const router = express.Router();
const { verifyToken, requireRole } = require("../auth/auth");
const { agregarAlCarrito, listarCarrito, eliminarDelCarrito, actualizarCantidad } = require("../controllers/ventasController");

// Solo rol "ventas"
router.use(verifyToken, requireRole("ventas"));

// Listar carrito
router.get("/", listarCarrito);

// Agregar producto al carrito (buscando por nombre y cantidad)
router.post("/agregar", agregarAlCarrito);

// Eliminar producto del carrito
router.delete("/:id", eliminarDelCarrito);

router.put("/:id", verifyToken, actualizarCantidad);





module.exports = router;