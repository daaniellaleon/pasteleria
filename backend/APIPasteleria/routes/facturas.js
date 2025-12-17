const express = require("express");
const router = express.Router();
const { getFacturas, getFacturaById, generarFacturaPDF } = require("../controllers/facturasController");

router.get("/", getFacturas);
router.get("/:id", getFacturaById);
router.get("/pdf/:id", generarFacturaPDF);

module.exports = router;
