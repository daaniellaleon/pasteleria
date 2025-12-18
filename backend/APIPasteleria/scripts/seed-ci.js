/**
 * CI Seed Script
 * Creates database, tables, and seeds for CI testing
 * Reads configuration from environment variables
 */

const mysql = require("mysql2/promise");
const bcrypt = require("bcrypt");

const DB_HOST = process.env.DB_HOST || "localhost";
const DB_PORT = process.env.DB_PORT || 3306;
const DB_USER = process.env.DB_USER || "root";
const DB_PASSWORD = process.env.DB_PASSWORD || "";
const DB_NAME = process.env.DB_NAME || "pasteleriadb";
const TEST_LOGIN_USER = process.env.TEST_LOGIN_USER || "testuser";
const TEST_LOGIN_PASSWORD = process.env.TEST_LOGIN_PASSWORD || "testpass";
const TEST_LOGIN_ROLE = process.env.TEST_LOGIN_ROLE || "ventas";

async function seedCI() {
  let connection;

  try {
    // Connect without database to create it if needed
    connection = await mysql.createConnection({
      host: DB_HOST,
      port: parseInt(DB_PORT, 10),
      user: DB_USER,
      password: DB_PASSWORD,
    });

    console.log("Connected to MySQL server");

    // 1. Create database if not exists
    await connection.execute(
      `CREATE DATABASE IF NOT EXISTS \`${DB_NAME}\` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci`
    );
    console.log(`Database '${DB_NAME}' ensured`);

    // Switch to the database
    await connection.changeUser({ database: DB_NAME });
    console.log(`Using database '${DB_NAME}'`);

    // 2. Create tables in correct order (respecting foreign keys)

    // Disable foreign key checks for table creation
    await connection.execute("SET FOREIGN_KEY_CHECKS = 0");

    // usuarios table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`usuarios\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`username\` varchar(50) NOT NULL,
        \`password\` varchar(255) NOT NULL,
        \`rol\` enum('ventas','inventario') NOT NULL,
        PRIMARY KEY (\`id\`),
        UNIQUE KEY \`username\` (\`username\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'usuarios' ensured");

    // categorias table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`categorias\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`nombre\` varchar(50) NOT NULL,
        PRIMARY KEY (\`id\`),
        UNIQUE KEY \`nombre\` (\`nombre\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'categorias' ensured");

    // productos table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`productos\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`nombre\` varchar(100) NOT NULL,
        \`categoria_id\` int NOT NULL,
        \`precio_unitario\` decimal(10,2) NOT NULL,
        \`stock\` int NOT NULL,
        \`estado\` enum('disponible','agotado') DEFAULT 'disponible',
        \`fecha_ingreso\` datetime DEFAULT CURRENT_TIMESTAMP,
        \`fecha_modificacion\` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        PRIMARY KEY (\`id\`),
        KEY \`categoria_id\` (\`categoria_id\`),
        CONSTRAINT \`productos_ibfk_1\` FOREIGN KEY (\`categoria_id\`) REFERENCES \`categorias\` (\`id\`) ON DELETE CASCADE
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'productos' ensured");

    // carrito table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`carrito\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`producto_id\` int NOT NULL,
        \`nombre\` varchar(100) NOT NULL,
        \`cantidad\` int NOT NULL,
        \`precio_unitario\` decimal(10,2) NOT NULL,
        \`total\` decimal(10,2) NOT NULL,
        \`usuario_id\` int NOT NULL,
        \`fecha\` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (\`id\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'carrito' ensured");

    // compras table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`compras\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`usuario_id\` int NOT NULL,
        \`fecha\` datetime DEFAULT CURRENT_TIMESTAMP,
        \`total\` decimal(10,2) NOT NULL,
        \`monto_recibido\` decimal(10,2) DEFAULT NULL,
        \`cambio\` decimal(10,2) DEFAULT NULL,
        \`metodo_pago\` enum('visa','mastercard','ach','yappy','efectivo') NOT NULL,
        \`status\` enum('success','pendiente','cancelado') NOT NULL,
        \`transaction_id\` varchar(50) NOT NULL,
        PRIMARY KEY (\`id\`),
        KEY \`usuario_id\` (\`usuario_id\`),
        CONSTRAINT \`compras_ibfk_1\` FOREIGN KEY (\`usuario_id\`) REFERENCES \`usuarios\` (\`id\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'compras' ensured");

    // detalle_compra table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`detalle_compra\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`compra_id\` int NOT NULL,
        \`producto_id\` int NOT NULL,
        \`nombre\` varchar(255) NOT NULL,
        \`cantidad\` int NOT NULL,
        \`precio_unitario\` decimal(10,2) NOT NULL,
        \`total\` decimal(10,2) NOT NULL,
        PRIMARY KEY (\`id\`),
        KEY \`compra_id\` (\`compra_id\`),
        KEY \`producto_id\` (\`producto_id\`),
        CONSTRAINT \`detalle_compra_ibfk_1\` FOREIGN KEY (\`compra_id\`) REFERENCES \`compras\` (\`id\`) ON DELETE CASCADE,
        CONSTRAINT \`detalle_compra_ibfk_2\` FOREIGN KEY (\`producto_id\`) REFERENCES \`productos\` (\`id\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'detalle_compra' ensured");

    // insumos table
    await connection.execute(`
      CREATE TABLE IF NOT EXISTS \`insumos\` (
        \`id\` int NOT NULL AUTO_INCREMENT,
        \`insumo\` varchar(100) NOT NULL,
        \`cantidad\` int NOT NULL,
        \`unidad_medida\` varchar(50) NOT NULL,
        \`proveedor\` varchar(100) NOT NULL,
        \`estado\` enum('disponible','agotado') DEFAULT 'disponible',
        \`fecha_ingreso\` datetime DEFAULT CURRENT_TIMESTAMP,
        \`fecha_modificacion\` datetime DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
        PRIMARY KEY (\`id\`)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
    `);
    console.log("Table 'insumos' ensured");

    // Re-enable foreign key checks
    await connection.execute("SET FOREIGN_KEY_CHECKS = 1");

    // 3. Insert seeds (idempotent)

    // Seed categorias with INSERT IGNORE (idempotent by unique nombre)
    await connection.execute(`
      INSERT IGNORE INTO \`categorias\` (\`id\`, \`nombre\`) VALUES (1, 'Galletas')
    `);
    await connection.execute(`
      INSERT IGNORE INTO \`categorias\` (\`id\`, \`nombre\`) VALUES (2, 'Pan')
    `);
    console.log("Categories seeded");

    // Seed test user with UPSERT (INSERT ... ON DUPLICATE KEY UPDATE)
    const testPasswordHash = await bcrypt.hash(TEST_LOGIN_PASSWORD, 10);
    await connection.execute(
      `
      INSERT INTO \`usuarios\` (\`username\`, \`password\`, \`rol\`)
      VALUES (?, ?, ?)
      ON DUPLICATE KEY UPDATE \`password\` = VALUES(\`password\`), \`rol\` = VALUES(\`rol\`)
    `,
      [TEST_LOGIN_USER, testPasswordHash, TEST_LOGIN_ROLE]
    );
    console.log(`Test user '${TEST_LOGIN_USER}' seeded with role '${TEST_LOGIN_ROLE}'`);

    // Also seed diegoventas for compatibility with existing data
    const diegoPasswordHash = await bcrypt.hash("diegoventas123", 10);
    await connection.execute(
      `
      INSERT INTO \`usuarios\` (\`username\`, \`password\`, \`rol\`)
      VALUES (?, ?, ?)
      ON DUPLICATE KEY UPDATE \`password\` = VALUES(\`password\`), \`rol\` = VALUES(\`rol\`)
    `,
      ["diegoventas", diegoPasswordHash, "ventas"]
    );
    console.log("User 'diegoventas' seeded");

    console.log("CI seed completed successfully!");
  } catch (error) {
    console.error("CI seed failed:", error.message);
    process.exit(1);
  } finally {
    if (connection) {
      await connection.end();
    }
  }
}

seedCI();
