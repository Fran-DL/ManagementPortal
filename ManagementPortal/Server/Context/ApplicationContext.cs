using System.Reflection.Emit;
using ManagementPortal.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Context
{
    /// <summary>
    /// Context de la aplicacion para manipular la base de datos.
    /// Justificacion: Se requiere Identity con SQL Server. Se incluyen otras entidades en el modelo.
    /// </summary>
    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationContext"/> class.
        /// </summary>
        /// <param name="options">Parametro por defecto para generar ApplicationContext.</param>
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Permisos que conforman los roles.
        /// </summary>
        public DbSet<ApplicationPermission> ApplicationPermissions { get; set; }

        /// <summary>
        /// Roles de los usuarios.
        /// </summary>
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }

        /// <summary>
        /// Productos de los usuarios.
        /// </summary>
        public DbSet<ApplicationUserProduct> ApplicationUserProducts { get; set; }

        /// <summary>
        /// Usuarios.
        /// </summary>
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        /// <summary>
        /// Metodos de 2FA disponibles en la aplicacion.
        /// </summary>
        public DbSet<TwoFactorMethod> TwoFactorMethods { get; set; }

        /// <summary>
        /// Asociacion de Usuario-Metodo.
        /// </summary>
        public DbSet<UserTwoFactorMethod> UserTwoFactorMethods { get; set; }

        /// <summary>
        /// Logs.
        /// </summary>
        public DbSet<ApplicationAuditLog> Logs { get; set; }

        /// <summary>
        /// Canales de chat para implementar mensajería.
        /// </summary>
        public DbSet<MessagingChannel> Channels { get; set; }

        /// <summary>
        /// Mensajes que envían los usuarios a los canales.
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// Mensajes recibidos por los usuarios.
        /// </summary>
        public DbSet<MessageReceiver> MessageReceivers { get; set; }

        /// <summary>
        /// Metodo para indicar caracteristicas del modelo de base de datos.
        /// </summary>
        /// <param name="builder">Parametro ModelBuilder por defecto.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationPermission>().ToTable("Permissions");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<UserTwoFactorMethod>().ToTable("UserTwoFactorMethods");
            builder.Entity<ApplicationUserProduct>().ToTable("UserProducts");
            builder.Entity<ApplicationAuditLog>().ToTable("Log");

            builder.Entity<MessagingChannel>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Channels)
                .UsingEntity(j => j.ToTable("ChannelsUsers"));

            builder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.SentMessages)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.Channel)
                .WithMany(c => c.Messages)
                .HasForeignKey("ChannelId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MessageReceiver>()
                .HasOne(mr => mr.Message)
                .WithMany(m => m.MessageReceivers)
                .HasForeignKey("MessageId");

            builder.Entity<MessageReceiver>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey("UserId");

            builder.Entity<ApplicationRole>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.ToTable("RolesPermissions"));
        }
    }
}