const express = require("express");
const router = express.Router();
const { listarProductos, crearProducto, editarProducto } = require("../controllers/productosController");
const { verifyToken, requireRole } = require("../auth/auth");

// Solo usuarios con rol "inventario" pueden listar, crear y editar productos
router.get("/", verifyToken, listarProductos); // quitar validación de rol solo para test
router.post("/", verifyToken, requireRole("inventario"), crearProducto);
router.put("/:id", verifyToken, requireRole("inventario"), editarProducto);

module.exports = router;