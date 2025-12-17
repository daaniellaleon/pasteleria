const express = require("express");
const { listarInsumos, crearInsumo, editarInsumo } = require("../controllers/insumosController");
const { verifyToken, requireRole } = require("../auth/auth");

const router = express.Router();

// Listar todos los insumos
router.get("/", verifyToken, listarInsumos);

// Crear insumo (solo admin o encargado, por ejemplo)
router.post("/", verifyToken, requireRole("inventario"), crearInsumo);

// Editar insumo
router.put("/:id", verifyToken, requireRole("inventario"), editarInsumo);

module.exports = router;
