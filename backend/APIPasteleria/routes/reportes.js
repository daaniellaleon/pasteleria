const express = require("express");
const router = express.Router();
const { reporteVentas, reporteInventario } = require("../controllers/reportesController");

// Ruta para reporte de ventas
router.get("/ventas", reporteVentas);

// Ruta para reporte de inventario
router.get("/inventario", reporteInventario);

module.exports = router;
