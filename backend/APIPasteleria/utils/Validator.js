class Validator {
  // Valida campos obligatorios
  static requiredFields(obj, fields) {
    for (const field of fields) {
      if (obj[field] === undefined || obj[field] === null || obj[field] === "") {
        return `El campo '${field}' es obligatorio`;
      }
    }
    return null;
  }

  // Valida que un número sea mayor o igual a cero
  static nonNegativeNumber(value, fieldName) {
    if (typeof value !== "number" || value <= 0) {
      return `El campo '${fieldName}' debe ser un número mayor que 0`;
    }
    return null;
  }

  // Valida que sea un entero no negativo
  static nonNegativeInteger(value, fieldName) {
    if (!Number.isInteger(value) || value <= 0) {
      return `El campo '${fieldName}' debe ser un número entero mayor que 0`;
    }
    return null;
  }

  
  // Valida enums
  static enumValue(value, fieldName, allowedValues) {
    if (!allowedValues.includes(value)) {
      return `El campo '${fieldName}' debe ser uno de: ${allowedValues.join(", ")}`;
    }
    return null;
  }
}

module.exports = Validator;
