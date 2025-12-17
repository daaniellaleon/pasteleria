const express = require("express");
require("dotenv").config();

const app = express();
app.use(express.json());

// Rutas
const usuariosRoutes = require("./routes/usuarios");
const categoriasRoutes = require("./routes/categorias");
const productosRoutes = require("./routes/productos");
const insumosRoutes = require("./routes/insumos");
const ventasRoutes = require("./routes/ventas");
const gestionarPagosRoutes = require("./routes/gestionarPagos");
const facturasRoutes = require("./routes/facturas");
const reportesRoutes = require("./routes/reportes");


app.use("/api/usuarios", usuariosRoutes);
app.use("/api/categorias", categoriasRoutes);
app.use("/api/productos", productosRoutes);
app.use("/api/insumos", insumosRoutes);
app.use("/api/ventas", ventasRoutes);
app.use("/api/gestionarPagos", gestionarPagosRoutes);
app.use("/api/facturas", facturasRoutes);
app.use("/api/reportes", reportesRoutes);

module.exports = app;
