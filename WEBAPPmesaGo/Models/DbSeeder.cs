using WEBAPPmesaGo.Models;
using System.Linq;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace WEBAPPmesaGo.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Migración Manual para Cupones (Evitar error 'no such table')
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Cupones"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Cupones"" PRIMARY KEY AUTOINCREMENT,
                    ""Codigo"" TEXT NOT NULL,
                    ""Descripcion"" TEXT NOT NULL,
                    ""Porcentaje"" INTEGER NOT NULL,
                    ""EsFeriado"" INTEGER NOT NULL,
                    ""FechaExpiracion"" TEXT NOT NULL,
                    ""Activo"" INTEGER NOT NULL
                );
            ");

            // Migración Manual para MovimientosInventario (Evitar error 'no such table')
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""MovimientosInventario"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_MovimientosInventario"" PRIMARY KEY AUTOINCREMENT,
                    ""IngredienteId"" INTEGER NOT NULL,
                    ""Fecha"" TEXT NOT NULL,
                    ""Cantidad"" TEXT NOT NULL,
                    ""Tipo"" TEXT NOT NULL,
                    ""Detalle"" TEXT NULL,
                    CONSTRAINT ""FK_MovimientosInventario_Ingredientes_IngredienteId"" FOREIGN KEY (""IngredienteId"") REFERENCES ""Ingredientes"" (""Id"") ON DELETE CASCADE
                );
            ");

            // Migración Manual para Clientes (Evitar error 'no such table')
            context.Database.ExecuteSqlRaw(@"
                CREATE TABLE IF NOT EXISTS ""Clientes"" (
                    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Clientes"" PRIMARY KEY AUTOINCREMENT,
                    ""Nombre"" TEXT NOT NULL,
                    ""Correo"" TEXT NOT NULL,
                    ""Password"" TEXT DEFAULT '',
                    ""TokenRecuperacion"" TEXT NULL,
                    ""FechaRegistro"" TEXT NOT NULL
                );
            ");

            // Migración Manual: Agregar columna Password si no existe (para evitar borrar datos anteriores)
            try
            {
                context.Database.ExecuteSqlRaw("SELECT Password FROM Clientes LIMIT 1");
            }
            catch
            {
                try 
                {
                    // SQLite syntax for adding column
                    context.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD COLUMN Password TEXT DEFAULT ''");
                    context.Database.ExecuteSqlRaw("ALTER TABLE Clientes ADD COLUMN TokenRecuperacion TEXT NULL");
                }
                catch { /* Ignorar si ya existen */ }
            }

            // ==========================================
            // 0. CUPONES POR DEFECTO (Reflejando los estáticos)
            // ==========================================
            // ==========================================
            // 0. CUPONES POR DEFECTO (Asegurar que existan)
            // ==========================================
            var cuponesDefecto = new List<Cupon>
            {
                new Cupon { 
                    Codigo = "ÑAÑITO", 
                    Descripcion = "Postre al 50% consumiendo en el local.", 
                    Porcentaje = 50, 
                    EsFeriado = false, 
                    Activo = true,
                    FechaExpiracion = DateTime.Now.AddYears(1) 
                },
                new Cupon { 
                    Codigo = "PIDE AQUÍ", 
                    Descripcion = "15% menos pagando en efectivo en el local.", 
                    Porcentaje = 15, 
                    EsFeriado = false, 
                    Activo = true,
                    FechaExpiracion = DateTime.Now.AddYears(1)
                },
                new Cupon { 
                    Codigo = "CHIRO PERO DIGNO", 
                    Descripcion = "10% OFF en plato del día entre 5pm y 6pm.", 
                    Porcentaje = 10, 
                    EsFeriado = false, 
                    Activo = true, 
                    FechaExpiracion = DateTime.Now.AddYears(1)
                }
            };

            foreach (var cupon in cuponesDefecto)
            {
                if (!context.Cupones.Any(c => c.Codigo == cupon.Codigo))
                {
                    context.Cupones.Add(cupon);
                }
            }
            // Guardamos cambios si se agrego alguno
            if (context.ChangeTracker.HasChanges())
            {
                context.SaveChanges();
            }

            // ==========================================
            // 1. INGREDIENTES (Consolidado)
            // ==========================================
            if (!context.Ingredientes.Any())
            {
                var ingredientes = new List<Ingrediente>
                {
                    // Básicos
                    new Ingrediente { Nombre = "Sal", Cantidad = 10, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Azúcar", Cantidad = 10, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Aceite", Cantidad = 20, Unidad = "litros", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Agua", Cantidad = 100, Unidad = "litros", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Ajo", Cantidad = 5, Unidad = "kg", StockMinimo = 0.5m },
                    new Ingrediente { Nombre = "Comino", Cantidad = 1, Unidad = "kg", StockMinimo = 0.2m },
                    new Ingrediente { Nombre = "Cebolla", Cantidad = 20, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Cebolla Morada", Cantidad = 10, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Limón", Cantidad = 100, Unidad = "unidades", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Cilantro", Cantidad = 2, Unidad = "atados", StockMinimo = 0.5m },
                    new Ingrediente { Nombre = "Tomate", Cantidad = 15, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Achiote", Cantidad = 1, Unidad = "kg", StockMinimo = 0.2m },
                    
                    // Harinas y Granos
                    new Ingrediente { Nombre = "Harina de Trigo", Cantidad = 20, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Arroz", Cantidad = 50, Unidad = "kg", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Mote", Cantidad = 20, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Maní Molido", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },

                    // Lacteos y Huevos
                    new Ingrediente { Nombre = "Queso Fresco", Cantidad = 20, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Huevos", Cantidad = 300, Unidad = "unidades", StockMinimo = 30 },
                    new Ingrediente { Nombre = "Leche", Cantidad = 20, Unidad = "litros", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Mantequilla", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Leche Evaporada", Cantidad = 10, Unidad = "latas", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Leche Condensada", Cantidad = 10, Unidad = "latas", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Crema de Leche", Cantidad = 10, Unidad = "litros", StockMinimo = 2 },

                    // Carnes y Proteínas
                    new Ingrediente { Nombre = "Carne de Res", Cantidad = 30, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Pollo", Cantidad = 50, Unidad = "kg", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Gallina Criolla", Cantidad = 10, Unidad = "unidades", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Cerdo", Cantidad = 40, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Pierna de Cerdo", Cantidad = 20, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Chivo", Cantidad = 15, Unidad = "kg", StockMinimo = 3 },
                    new Ingrediente { Nombre = "Pescado (Albacora)", Cantidad = 20, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Pescado (Varios)", Cantidad = 15, Unidad = "kg", StockMinimo = 3 },
                    new Ingrediente { Nombre = "Camarón", Cantidad = 20, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Salchicha", Cantidad = 10, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Mondongo", Cantidad = 15, Unidad = "kg", StockMinimo = 3 },
                    new Ingrediente { Nombre = "Carne Molida", Cantidad = 10, Unidad = "kg", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Chicharrón", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },

                    // Verduras y Frutas
                    new Ingrediente { Nombre = "Papa", Cantidad = 100, Unidad = "kg", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Plátano Verde", Cantidad = 100, Unidad = "unidades", StockMinimo = 20 },
                    new Ingrediente { Nombre = "Plátano Maduro", Cantidad = 80, Unidad = "unidades", StockMinimo = 15 },
                    new Ingrediente { Nombre = "Yuca", Cantidad = 30, Unidad = "kg", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Choclo", Cantidad = 30, Unidad = "unidades", StockMinimo = 5 },
                    new Ingrediente { Nombre = "Hojas de Choclo", Cantidad = 50, Unidad = "unidades", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Higos", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Coco (Leche)", Cantidad = 5, Unidad = "litros", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Mortiño", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Piña", Cantidad = 10, Unidad = "unidades", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Mora", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },
                    
                    // Otros
                    new Ingrediente { Nombre = "Cerveza", Cantidad = 24, Unidad = "botellas", StockMinimo = 6 },
                    new Ingrediente { Nombre = "Panela", Cantidad = 10, Unidad = "bloques", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Café Molido", Cantidad = 5, Unidad = "kg", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Colita Roja", Cantidad = 24, Unidad = "botellas", StockMinimo = 6 },
                    new Ingrediente { Nombre = "Bizcocho", Cantidad = 10, Unidad = "unidades", StockMinimo = 2 },
                    new Ingrediente { Nombre = "Pan Hamburguesa", Cantidad = 30, Unidad = "unidades", StockMinimo = 10 },
                    new Ingrediente { Nombre = "Salsas (Mayonesa/Tomate)", Cantidad = 5, Unidad = "litros", StockMinimo = 1 },
                    new Ingrediente { Nombre = "Hielo", Cantidad = 10, Unidad = "bolsas", StockMinimo = 2 }
                };

                context.Ingredientes.AddRange(ingredientes);
                context.SaveChanges();
            }

            // ===============================================
            // FORCE UPDATE FOR TESTING: Set all stock to 100
            // ===============================================
            // This block ensures that even if ingredients exist, their stock is reset to 100 as requested
            var allIngredients = context.Ingredientes.ToList();
            foreach (var ing in allIngredients)
            {
                if (ing.Cantidad != 100) 
                {
                    ing.Cantidad = 100;
                }
            }
            if (context.ChangeTracker.HasChanges())
            {
                context.SaveChanges();
            }

            // ==========================================
            // 2. PLATOS Y RECETAS
            // ==========================================
            // Nota: Si ya existen platos, actualizamos su Preparacion. Si no, los creamos.
            // Para simplicidad en este Seed, asumimos que si está vacío creamos todo. 
            // Si ya existe, intentaremos actualizar la Preparacion.
            
            var platosData = new List<Plato>
            {
                // ENTRADAS
                new Plato { 
                    Nombre = "Empanadas de viento", 
                    Categoria = "Entradas", Precio = 1.50m, 
                    Descripcion = "Crujientes y rellenas de queso, con azúcar.", ImagenUrl = "emepanadas_viento.png",
                    Preparacion = "Ingredientes:\n- Harina de trigo\n- Agua tibia\n- Sal\n- Queso fresco rallado\n- Aceite vegetal\n- Azúcar (opcional)\n"
                },
                new Plato { 
                    Nombre = "Empanadas de verde", 
                    Categoria = "Entradas", Precio = 2.00m, 
                    Descripcion = "Masa de plátano verde rellena de queso o carne.", ImagenUrl = "emepanadas_verde.png",
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Queso o carne cocida\n- Ajo\n- Sal\n- Aceite\n"
                },
                new Plato {
                    Nombre = "Humitas", Categoria = "Entradas", Precio = 1.75m,
                    Descripcion = "Suave masa de choclo al vapor con queso.", ImagenUrl = "humitas.png",
                    Preparacion = "Ingredientes:\n- Choclo tierno molido\n- Huevos\n- Mantequilla\n- Queso\n- Sal\n- Hojas de choclo\n"
                },
                new Plato {
                    Nombre = "Corviches", Categoria = "Entradas", Precio = 2.50m,
                    Descripcion = "De pescado con maní y plátano verde.", ImagenUrl = "corviche.png",
                    Preparacion = "Ingredientes:\n- Plátano verde rallado\n- Maní molido\n- Pescado desmenuzado\n- Ajo\n- Cebolla\n- Comino\n- Aceite\n"
                },
                new Plato {
                    Nombre = "Choclo con queso", Categoria = "Entradas", Precio = 2.50m,
                    Descripcion = "Mazorca tierna cocinada con salsa de queso.", ImagenUrl = "choclo.png",
                    Preparacion = "Ingredientes:\n- Choclo\n- Queso fresco\n- Sal\n"
                },

                // SOPAS
                new Plato {
                    Nombre = "Caldo de bola", Categoria = "Sopas", Precio = 5.50m,
                    Descripcion = "Sopa consistente con bola de plátano rellena.", ImagenUrl = "caldo_bola.png",
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Carne de res\n- Maní molido\n- Yuca\n- Ajo\n- Cebolla\n- Cilantro\n- Comino\n"
                },
                new Plato {
                    Nombre = "Encebollado", Categoria = "Sopas", Precio = 4.00m,
                    Descripcion = "El clásico guayaco. Albacora, yuca y curtido.", ImagenUrl = "encebollado.png",
                    Preparacion = "Ingredientes:\n- Albacora\n- Yuca\n- Cebolla morada\n- Limón\n- Cilantro\n- Comino\n"
                },
                new Plato {
                    Nombre = "Locro de papa", Categoria = "Sopas", Precio = 4.50m,
                    Descripcion = "Sopa cremosa con queso y aguacate.", ImagenUrl = "locro.png",
                    Preparacion = "Ingredientes:\n- Papa\n- Leche\n- Queso\n- Achiote\n- Cebolla\n- Ajo\n"
                },
                new Plato {
                    Nombre = "Caldo de gallina", Categoria = "Sopas", Precio = 4.50m,
                    Descripcion = "Criollo, para levantar el ánimo.", ImagenUrl = "caldo_gallina.png",
                    Preparacion = "Ingredientes:\n- Gallina criolla\n- Papa\n- Cebolla\n- Ajo\n- Cilantro\n"
                },

                // PLATOS FUERTES
                new Plato {
                    Nombre = "Seco de pollo", Categoria = "Platos Fuertes", Precio = 6.00m,
                    Descripcion = "Estofado jugoso con arroz amarillo y maduro.", ImagenUrl = "seco_pollo.png",
                    Preparacion = "Ingredientes:\n- Pollo\n- Cerveza\n- Cebolla\n- Ajo\n- Cilantro\n- Comino\n- Arroz\n- Maduro\n"
                },
                new Plato {
                    Nombre = "Seco de chivo", Categoria = "Platos Fuertes", Precio = 8.50m,
                    Descripcion = "Tradicional, macerado en chicha y cerveza.", ImagenUrl = "seco_chivo.png",
                    Preparacion = "Ingredientes:\n- Chivo\n- Cerveza o chicha\n- Ajo\n- Cebolla\n- Cilantro\n"
                },
                new Plato {
                    Nombre = "Fritada", Categoria = "Platos Fuertes", Precio = 9.00m,
                    Descripcion = "Cerdo dorado con mote, papas y maduro.", ImagenUrl = "fritada.png",
                    Preparacion = "Ingredientes:\n- Cerdo\n- Ajo\n- Comino\n- Sal\n- Mote\n- Papas\n- Maduro\n"
                },
                new Plato {
                    Nombre = "Hornado", Categoria = "Platos Fuertes", Precio = 9.50m,
                    Descripcion = "Pierna de cerdo al horno con agrio y tortillas.", ImagenUrl = "hornado.png",
                    Preparacion = "Ingredientes:\n- Pierna de cerdo\n- Ajo\n- Cerveza\n- Comino\n- Sal\n"
                },
                new Plato {
                    Nombre = "Ceviche mixto", Categoria = "Platos Fuertes", Precio = 10.00m,
                    Descripcion = "Camarón y pescado curtido en limón.", ImagenUrl = "ceviche_mixto.png",
                    Preparacion = "Ingredientes:\n- Camarón\n- Pescado\n- Limón\n- Cebolla\n- Tomate\n- Cilantro\n"
                },
                new Plato {
                    Nombre = "Ceviche de camarón", Categoria = "Platos Fuertes", Precio = 9.50m,
                    Descripcion = "Fresco y delicioso con chifles.", ImagenUrl = "ceviche_camaron.png",
                    Preparacion = "Ingredientes:\n- Camarón\n- Limón\n- Tomate\n- Cebolla\n- Cilantro\n" 
                },
                new Plato {
                    Nombre = "Bandera ecuatoriana", Categoria = "Platos Fuertes", Precio = 11.00m,
                    Descripcion = "Guatita, seco y ceviche en un solo plato.", ImagenUrl = "bandera.png",
                    Preparacion = "Ingredientes:\n- Guatita\n- Seco\n- Ceviche\n"
                },

                // RÁPIDOS
                 new Plato {
                    Nombre = "Tigrillo", Categoria = "Platos Rápidos", Precio = 5.00m,
                    Descripcion = "Verde majado con queso, chicharrón y huevo.", ImagenUrl = "tigrillo.png",
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Queso fresco\n- Huevos\n- Chicharrón\n- Mantequilla\n- Sal\n"
                },
                new Plato {
                    Nombre = "Bolón mixto", Categoria = "Platos Rápidos", Precio = 3.50m,
                    Descripcion = "Bola de verde con queso y chicharrón.", ImagenUrl = "bolon.png",
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Queso fresco\n- Chicharrón\n- Ajo\n- Sal\n- Aceite (opcional)\n"
                },
                new Plato {
                    Nombre = "Arroz con huevo", Categoria = "Platos Rápidos", Precio = 3.00m,
                    Descripcion = "El salvavidas, sencillo y rico.", ImagenUrl = "arroz_huevo.png",
                    Preparacion = "Ingredientes:\n- Arroz\n- Agua\n- Sal\n- Huevos\n- Aceite\n"
                },
                new Plato {
                    Nombre = "Guatita", Categoria = "Platos Rápidos", Precio = 5.50m,
                    Descripcion = "Mondongo en salsa de maní con papas.", ImagenUrl = "guatitta.png",
                    Preparacion = "Ingredientes:\n- Mondongo\n- Maní tostado y molido\n- Papa\n- Achiote\n- Cebolla\n- Ajo\n- Comino\n- Cilantro\n- Sal\n"
                },
                new Plato {
                    Nombre = "Hamburguesa criolla", Categoria = "Platos Rápidos", Precio = 6.50m,
                    Descripcion = "Carne casera con salsas de la casa.", ImagenUrl = "hamburguesa.png",
                    Preparacion = "Ingredientes:\n- Carne molida de res\n- Ajo\n- Comino\n- Sal\n- Pan para hamburguesa\n- Queso\n- Lechuga\n- Tomate\n- Mayonesa y salsa de tomate\n"
                },

                // DESAYUNOS
                new Plato { 
                    Nombre = "Tigrillo clásico", 
                    Categoria = "Desayunos", Precio = 5.50m, 
                    Descripcion = "Solo con queso y huevo frito.", ImagenUrl = "tigrillo_clasico.png", 
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Queso fresco\n- Huevos\n- Mantequilla\n- Sal\n"
                },
                new Plato { 
                    Nombre = "Encebollado matutino", 
                    Categoria = "Desayunos", Precio = 4.00m, 
                    Descripcion = "Ideal para curar la resaca.", ImagenUrl = "encebollado1.png", 
                    Preparacion = "Ingredientes:\n- Albacora\n- Yuca\n- Cebolla morada\n- Limón\n- Cilantro\n- Comino\n- Sal\n"
                },
                new Plato { 
                    Nombre = "Bolón con café pasado", 
                    Categoria = "Desayunos", Precio = 4.50m, 
                    Descripcion = "Desayuno costeño tradicional.", ImagenUrl = "cafe_bolon.png", 
                    Preparacion = "Ingredientes:\n- Plátano verde\n- Queso o chicharrón\n- Ajo\n- Sal\n- Café molido\n- Agua\n"
                },
                new Plato { 
                    Nombre = "Caldo de salchicha", 
                    Categoria = "Desayunos", Precio = 5.00m, 
                    Descripcion = "Intenso y lleno de sabor.", ImagenUrl = "caldo_salchicha.png", 
                    Preparacion = "Ingredientes:\n- Salchicha\n- Papa\n- Cebolla\n- Ajo\n- Cilantro\n- Sal\n"
                },

                // POSTRES
                 new Plato { 
                    Nombre = "Dulce de higos con queso", Categoria = "Postres", Precio = 3.00m, 
                    Descripcion = "Higos en miel de panela.", ImagenUrl = "higos.png", 
                    Preparacion = "Ingredientes:\n- Higos\n- Panela\n- Canela\n- Agua\n- Queso fresco\n"
                 },
                 new Plato { 
                    Nombre = "Tres leches", Categoria = "Postres", Precio = 3.50m, 
                    Descripcion = "Bizcocho mojado y cremoso.", ImagenUrl = "tres_leches.png", 
                    Preparacion = "Ingredientes:\n- Bizcocho\n- Leche evaporada\n- Leche condensada\n- Crema de leche\n- Vainilla\n"
                },
                 new Plato { 
                    Nombre = "Flan de coco", Categoria = "Postres", Precio = 2.50m, 
                    Descripcion = "Suave y tropical.", ImagenUrl = "flan_coco.png", 
                    Preparacion = "Ingredientes:\n- Leche de coco\n- Huevos\n- Azúcar\n- Vainilla\n"
                },
                 new Plato { 
                    Nombre = "Helado de paila", Categoria = "Postres", Precio = 2.50m, 
                    Descripcion = "Tradicional de frutas.", ImagenUrl = "helado.png", 
                    Preparacion = "Ingredientes:\n- Fruta natural\n- Azúcar\n- Hielo\n- Sal\n"
                },

                // BEBIDAS
                new Plato { 
                    Nombre = "Colada morada", Categoria = "Bebidas", Precio = 2.50m, 
                    Descripcion = "Bebida ancestral.", ImagenUrl = "colada_morada.png", 
                    Preparacion = "Ingredientes:\n- Mortiño\n- Piña\n- Mora\n- Harina\n- Canela\n- Clavo de olor\n- Panela\n"
                },
                new Plato { 
                    Nombre = "Jugo natural", Categoria = "Bebidas", Precio = 2.00m, 
                    Descripcion = "Sabores varios.", ImagenUrl = "jugo.png", 
                    Preparacion = "Ingredientes:\n- Fruta fresca\n- Agua\n- Azúcar (opcional)\n"
                },
                new Plato { 
                    Nombre = "Café pasado", Categoria = "Bebidas", Precio = 1.50m, 
                    Descripcion = "Café de Loja filtrado.", ImagenUrl = "cafe.png", 
                    Preparacion = "Ingredientes:\n- Café molido\n- Agua caliente\n"
                },
                new Plato { 
                    Nombre = "Colita roja", Categoria = "Bebidas", Precio = 1.00m, 
                    Descripcion = "Sabor fresa.", ImagenUrl = "colita.png", 
                    Preparacion = "Ingredientes:\n- Bebida gaseosa\n"
                },

                // EXTRAS
                new Plato { 
                    Nombre = "Porción de Arroz", Categoria = "Extras", Precio = 1.00m, 
                    Descripcion = "Arroz blanco.", ImagenUrl = "arroz.png", 
                    Preparacion = "Ingredientes:\n- Arroz\n- Agua\n- Sal\n"
                },
                new Plato { 
                    Nombre = "Mote pillo", Categoria = "Extras", Precio = 2.50m, 
                    Descripcion = "Mote con huevo.", ImagenUrl = "mote.png", 
                    Preparacion = "Ingredientes:\n- Mote\n- Huevos\n- Cebolla\n- Mantequilla\n- Sal\n"
                },
                new Plato { 
                    Nombre = "Papas fritas", Categoria = "Extras", Precio = 2.00m, 
                    Descripcion = "Porción mediana.", ImagenUrl = "papas.png", 
                    Preparacion = "Ingredientes:\n- Papa\n- Aceite\n- Sal\n"
                },
                new Plato { 
                    Nombre = "Maduro frito", Categoria = "Extras", Precio = 1.50m, 
                    Descripcion = "Dulce y suave.", ImagenUrl = "maduro.png", 
                    Preparacion = "Ingredientes:\n- Plátano maduro\n- Aceite\n"
                },
                new Plato { 
                    Nombre = "Ají casero", Categoria = "Extras", Precio = 0.50m, 
                    Descripcion = "Picante.", ImagenUrl = "aji.png", 
                    Preparacion = "Ingredientes:\n- Ají\n- Tomate\n- Cebolla\n- Limón\n- Sal\n"
                },

                 // COMBOS
                new Plato { 
                    Nombre = "Yapa de la casa", Categoria = "Combos", Precio = 13.50m, 
                    Descripcion = "Plato fuerte + extra + sorpresa.", ImagenUrl = "yapa.png", 
                    Preparacion = "Incluye:\n- 1 plato fuerte a elección\n- 1 extra\n- Yapa sorpresa\n\nPreparación:\n1. Preparar el plato fuerte seleccionado.\n2. Añadir el extra elegido.\n3. Incluir una porción adicional sorpresa." 
                },
                new Plato { 
                    Nombre = "No hay plata pero hay hambre", Categoria = "Combos", Precio = 4.99m, 
                    Descripcion = "Arroz, huevo, papas, colita.", ImagenUrl = "nohayplata.png", 
                    Preparacion = "Incluye:\n- Arroz con huevo\n- Papas fritas\n- Colita roja\n\nPreparación:\n1. Cocinar arroz blanco.\n2. Freír huevo.\n3. Preparar papas fritas.\n4. Servir acompañado de colita roja fría." 
                },
                new Plato { 
                    Nombre = "Ñañito surtido", Categoria = "Combos", Precio = 9.99m, 
                    Descripcion = "Entrada + rápido + bebida.", ImagenUrl = "nanito.png", 
                    Preparacion = "Incluye:\n- 1 entrada\n- 1 plato rápido\n- 1 bebida\n\nPreparación:\n1. Preparar la entrada seleccionada.\n2. Preparar el plato rápido elegido.\n3. Servir con la bebida." 
                },
                new Plato { 
                    Nombre = "Ni la dieta me detiene", Categoria = "Combos", Precio = 12.50m, 
                    Descripcion = "Llapingachos, fritada, huevo, café/jugo.", ImagenUrl = "dieta.png", 
                    Preparacion = "Incluye:\n- Llapingachos\n- Fritadita\n- Huevo\n- Café o jugo\n\nPreparación:\n1. Preparar llapingachos dorados.\n2. Cocinar fritada.\n3. Freír huevo.\n4. Servir con café pasado o jugo natural." 
                }
            };

            if (!context.Platos.Any())
            {
                context.Platos.AddRange(platosData);
                context.SaveChanges();
            }
            else
            {
                // Si ya existen, actualizamos Preparacion
                foreach (var p in platosData)
                {
                    var existing = context.Platos.FirstOrDefault(x => x.Nombre == p.Nombre);
                    if (existing != null)
                    {
                        existing.Preparacion = p.Preparacion;
                    }
                }
                context.SaveChanges();
            }

            // ==========================================
            // 3. RECETAS (Relación Plato - Ingrediente)
            // ==========================================
            // ==========================================
            // 3. RECETAS (Relación Plato - Ingrediente)
            // ==========================================
            // Definición masiva de recetas (Plato, Ingrediente, Cantidad)
            var recetasData = new List<(string Plato, string Ingrediente, decimal Cantidad)>
            {
                // -- ENTRADAS --
                ("Empanadas de viento", "Harina de Trigo", 0.1m), ("Empanadas de viento", "Queso Fresco", 0.05m), ("Empanadas de viento", "Aceite", 0.05m),
                ("Empanadas de verde", "Plátano Verde", 1m), ("Empanadas de verde", "Queso Fresco", 0.05m), ("Empanadas de verde", "Aceite", 0.05m),
                ("Humitas", "Choclo", 2m), ("Humitas", "Queso Fresco", 0.05m), ("Humitas", "Huevos", 0.5m),
                ("Corviches", "Plátano Verde", 1m), ("Corviches", "Pescado (Albacora)", 0.05m), ("Corviches", "Maní Molido", 0.03m), ("Corviches", "Aceite", 0.1m),
                ("Choclo con queso", "Choclo", 1m), ("Choclo con queso", "Queso Fresco", 0.1m),

                // -- SOPAS --
                ("Caldo de bola", "Plátano Verde", 1.5m), ("Caldo de bola", "Carne de Res", 0.15m), ("Caldo de bola", "Maní Molido", 0.05m), ("Caldo de bola", "Yuca", 0.1m),
                ("Encebollado", "Pescado (Albacora)", 0.15m), ("Encebollado", "Yuca", 0.2m), ("Encebollado", "Cebolla Morada", 0.1m), ("Encebollado", "Limón", 2m), ("Encebollado", "Cilantro", 0.05m),
                ("Locro de papa", "Papa", 0.3m), ("Locro de papa", "Queso Fresco", 0.1m), ("Locro de papa", "Leche", 0.1m), ("Locro de papa", "Achiote", 0.01m),
                ("Caldo de gallina", "Gallina Criolla", 0.25m), ("Caldo de gallina", "Papa", 0.2m), ("Caldo de gallina", "Cilantro", 0.05m),

                // -- PLATOS FUERTES --
                ("Seco de pollo", "Pollo", 0.35m), ("Seco de pollo", "Arroz", 0.2m), ("Seco de pollo", "Cerveza", 0.2m), ("Seco de pollo", "Plátano Maduro", 1m),
                ("Seco de chivo", "Chivo", 0.3m), ("Seco de chivo", "Cerveza", 0.2m), ("Seco de chivo", "Arroz", 0.2m), ("Seco de chivo", "Achiote", 0.02m),
                ("Fritada", "Cerdo", 0.4m), ("Fritada", "Mote", 0.15m), ("Fritada", "Papa", 0.2m), ("Fritada", "Plátano Maduro", 1m),
                ("Hornado", "Pierna de Cerdo", 0.4m), ("Hornado", "Mote", 0.15m), ("Hornado", "Papa", 0.2m),
                ("Ceviche mixto", "Camarón", 0.15m), ("Ceviche mixto", "Pescado (Varios)", 0.15m), ("Ceviche mixto", "Limón", 3m), ("Ceviche mixto", "Cebolla", 0.1m), ("Ceviche mixto", "Tomate", 0.1m),
                ("Ceviche de camarón", "Camarón", 0.25m), ("Ceviche de camarón", "Limón", 3m), ("Ceviche de camarón", "Cebolla", 0.1m), ("Ceviche de camarón", "Tomate", 0.1m),
                ("Bandera ecuatoriana", "Mondongo", 0.15m), ("Bandera ecuatoriana", "Chivo", 0.15m), ("Bandera ecuatoriana", "Camarón", 0.1m), ("Bandera ecuatoriana", "Arroz", 0.2m),

                // -- RÁPIDOS --
                ("Tigrillo", "Plátano Verde", 2.5m), ("Tigrillo", "Queso Fresco", 0.15m), ("Tigrillo", "Huevos", 2m), ("Tigrillo", "Chicharrón", 0.1m),
                ("Bolón mixto", "Plátano Verde", 2m), ("Bolón mixto", "Queso Fresco", 0.1m), ("Bolón mixto", "Chicharrón", 0.1m),
                ("Arroz con huevo", "Arroz", 0.25m), ("Arroz con huevo", "Huevos", 2m), ("Arroz con huevo", "Aceite", 0.05m),
                ("Guatita", "Mondongo", 0.25m), ("Guatita", "Maní Molido", 0.1m), ("Guatita", "Papa", 0.2m), ("Guatita", "Arroz", 0.2m),
                ("Hamburguesa criolla", "Carne Molida", 0.2m), ("Hamburguesa criolla", "Pan Hamburguesa", 1m), ("Hamburguesa criolla", "Queso Fresco", 0.05m), ("Hamburguesa criolla", "Papa", 0.2m),

                // -- DESAYUNOS --
                ("Tigrillo clásico", "Plátano Verde", 2m), ("Tigrillo clásico", "Queso Fresco", 0.1m), ("Tigrillo clásico", "Huevos", 1m),
                ("Encebollado matutino", "Pescado (Albacora)", 0.15m), ("Encebollado matutino", "Yuca", 0.2m), ("Encebollado matutino", "Cebolla Morada", 0.1m), ("Encebollado matutino", "Limón", 2m),
                ("Bolón con café pasado", "Plátano Verde", 2m), ("Bolón con café pasado", "Queso Fresco", 0.1m), ("Bolón con café pasado", "Café Molido", 0.03m),
                ("Caldo de salchicha", "Salchicha", 0.25m), ("Caldo de salchicha", "Papa", 0.15m), ("Caldo de salchicha", "Cebolla", 0.1m),

                // -- POSTRES --
                ("Dulce de higos con queso", "Higos", 0.2m), ("Dulce de higos con queso", "Panela", 0.1m), ("Dulce de higos con queso", "Queso Fresco", 0.05m),
                ("Tres leches", "Bizcocho", 1m), ("Tres leches", "Leche", 0.1m), ("Tres leches", "Leche Condensada", 0.2m), ("Tres leches", "Leche Evaporada", 0.2m),
                ("Flan de coco", "Coco (Leche)", 0.2m), ("Flan de coco", "Huevos", 1m), ("Flan de coco", "Azúcar", 0.1m),
                ("Helado de paila", "Mora", 0.15m), ("Helado de paila", "Hielo", 0.2m), ("Helado de paila", "Azúcar", 0.05m),

                // -- BEBIDAS --
                ("Colada morada", "Mortiño", 0.05m), ("Colada morada", "Mora", 0.05m), ("Colada morada", "Piña", 0.1m), ("Colada morada", "Panela", 0.05m),
                ("Jugo natural", "Piña", 0.2m), ("Jugo natural", "Azúcar", 0.05m),
                ("Café pasado", "Café Molido", 0.03m),
                ("Colita roja", "Colita Roja", 1m),

                // -- EXTRAS --
                ("Porción de Arroz", "Arroz", 0.2m),
                ("Mote pillo", "Mote", 0.2m), ("Mote pillo", "Huevos", 1m),
                ("Papas fritas", "Papa", 0.3m), ("Papas fritas", "Aceite", 0.05m),
                ("Maduro frito", "Plátano Maduro", 1m),
                ("Ají casero", "Tomate", 0.1m), ("Ají casero", "Cebolla", 0.05m),

                // -- COMBOS --
                ("Yapa de la casa", "Pollo", 0.2m), ("Yapa de la casa", "Arroz", 0.2m),
                ("No hay plata pero hay hambre", "Arroz", 0.2m), ("No hay plata pero hay hambre", "Huevos", 1m), ("No hay plata pero hay hambre", "Papa", 0.15m), ("No hay plata pero hay hambre", "Colita Roja", 1m),
                ("Ñañito surtido", "Harina de Trigo", 0.1m), ("Ñañito surtido", "Plátano Verde", 1m), ("Ñañito surtido", "Colita Roja", 1m),
                ("Ni la dieta me detiene", "Papa", 0.3m), ("Ni la dieta me detiene", "Cerdo", 0.2m), ("Ni la dieta me detiene", "Huevos", 1m), ("Ni la dieta me detiene", "Café Molido", 0.02m)
            };

            foreach (var r in recetasData)
            {
                var plato = context.Platos.FirstOrDefault(p => p.Nombre == r.Plato);
                var ingrediente = context.Ingredientes.FirstOrDefault(i => i.Nombre == r.Ingrediente);

                if (plato != null && ingrediente != null)
                {
                    if (!context.PlatoIngredientes.Any(pi => pi.PlatoId == plato.Id && pi.IngredienteId == ingrediente.Id))
                    {
                        context.PlatoIngredientes.Add(new PlatoIngrediente
                        {
                            PlatoId = plato.Id,
                            IngredienteId = ingrediente.Id,
                            CantidadRequerida = r.Cantidad
                        });
                    }
                }
            }
            context.SaveChanges();
        }
    }
}