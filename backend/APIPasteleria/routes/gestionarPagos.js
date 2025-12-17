// routes/ventas.js
const express = require("express");
const router = express.Router();
const { verifyToken, requireRole } = require("../auth/auth");
const { seleccionarMetodo, pagarEfectivo, pagarConMetodoDirecto } = require("../controllers/gestionarPagos");

// Solo rol "ventas" puede acceder a estas rutas
router.use(verifyToken, requireRole("ventas"));

// Endpoint para procesar pago
router.post("/pagar/:metodo", (req, res) => {
    const metodo = req.params.metodo?.toLowerCase();
    pagarConMetodoDirecto(req, res, metodo);
});


router.post("/seleccionar", seleccionarMetodo);
router.post("/pagar-efectivo", pagarEfectivo);


module.exports = router;
