const jwt = require('jsonwebtoken');
const dotenv = require('dotenv');
dotenv.config();

const jwtSecret = process.env.JWT_SECRET || 'secret';

// Función para generar token JWT
function generarToken(usuario) {
  // usuario = { id, username, rol }
  return jwt.sign(
    { id: usuario.id, username: usuario.username, rol: usuario.rol },
    jwtSecret,
    { expiresIn: '1h' } // token válido por 1 hora
  );
}

// Middleware para verificar token
function verifyToken(req, res, next) {
  const authHeader = req.headers.authorization;
  if (!authHeader) return res.status(401).json({ message: 'Token missing' });

  const token = authHeader.split(' ')[1];
  if (!token) return res.status(401).json({ message: 'Token missing' });

  try {
    const payload = jwt.verify(token, jwtSecret);
    req.user = payload; // { id, username, rol, iat, exp }
    next();
  } catch (err) {
    return res.status(401).json({ message: 'Token inválido' });
  }
}

// Middleware para validar roles
function requireRole(...allowedRoles) {
  return (req, res, next) => {
    if (!req.user) return res.status(401).json({ message: 'No autenticado' });
    if (!allowedRoles.includes(req.user.rol)) {
      return res.status(403).json({ message: 'Acceso denegado' });
    }
    next();
  };
}

module.exports = { generarToken, verifyToken, requireRole };
