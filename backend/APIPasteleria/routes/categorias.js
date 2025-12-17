const express = require("express");
const router = express.Router();
const { listarCategorias } = require("../controllers/categoriasController");
const { verifyToken, requireRole } = require("../auth/auth");

router.get("/", verifyToken, requireRole("inventario"), listarCategorias);


module.exports = router;
