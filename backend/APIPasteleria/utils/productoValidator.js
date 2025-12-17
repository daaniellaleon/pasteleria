const Validator = require("./Validator"); // o la ruta correcta a Validator.js

// Función que valida los campos de un producto
function validarProducto(data) {
  // Campos obligatorios
  let error = Validator.requiredFields(data, ["nombre", "categoria", "precio_unitario", "stock"]);
  if (error) return error;

  // Validaciones numéricas
  error = Validator.nonNegativeNumber(data.precio_unitario, "precio_unitario");
  if (error) return error;

  error = Validator.nonNegativeInteger(data.stock, "stock");
  if (error) return error;

  return null; // todo OK
}

module.exports = validarProducto;
