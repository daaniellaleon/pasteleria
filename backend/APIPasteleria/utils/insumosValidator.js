const Validator = require("./Validator"); // Ajusta la ruta según tu proyecto

// Función que valida los campos de un insumo
function validarInsumo(data) {
  // Campos obligatorios
  let error = Validator.requiredFields(data, ["insumo", "cantidad", "unidad_medida", "proveedor"]);
  if (error) return error;

  // Validaciones numéricas
  error = Validator.nonNegativeInteger(data.cantidad, "cantidad");
  if (error) return error;

  return null; // todo OK
}

module.exports = validarInsumo;
