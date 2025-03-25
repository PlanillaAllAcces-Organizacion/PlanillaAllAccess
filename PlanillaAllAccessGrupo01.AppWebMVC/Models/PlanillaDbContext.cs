using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class PlanillaDbContext : DbContext
{
    public PlanillaDbContext()
    {
    }

    public PlanillaDbContext(DbContextOptions<PlanillaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsignacionBono> AsignacionBonos { get; set; }

    public virtual DbSet<AsignacionDescuento> AsignacionDescuentos { get; set; }

    public virtual DbSet<Bono> Bonos { get; set; }

    public virtual DbSet<ControlAsistencium> ControlAsistencia { get; set; }

    public virtual DbSet<Descuento> Descuentos { get; set; }

    public virtual DbSet<DescuentoPlanilla> DescuentoPlanillas { get; set; }

    public virtual DbSet<DevengoPlanilla> DevengoPlanillas { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<EmpleadoPlanilla> EmpleadoPlanillas { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<Planilla> Planillas { get; set; }

    public virtual DbSet<PuestoTrabajo> PuestoTrabajos { get; set; }

    public virtual DbSet<TipoPlanilla> TipoPlanillas { get; set; }

    public virtual DbSet<TipodeHorario> TipodeHorarios { get; set; }

    public virtual DbSet<Vacacion> Vacacions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsignacionBono>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asignaci__3214EC072035D774");

            entity.ToTable("AsignacionBono");

            entity.HasOne(d => d.Bonos).WithMany(p => p.AsignacionBonos)
                .HasForeignKey(d => d.BonosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AsignacionBonos_Bonos");

            entity.HasOne(d => d.Empleados).WithMany(p => p.AsignacionBonos)
                .HasForeignKey(d => d.EmpleadosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AsignacionBonos_Empleados");
        });

        modelBuilder.Entity<AsignacionDescuento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asignaci__3214EC07518B75A0");

            entity.ToTable("AsignacionDescuento");

            entity.HasOne(d => d.Descuentos).WithMany(p => p.AsignacionDescuentos)
                .HasForeignKey(d => d.DescuentosId)
                .HasConstraintName("FK_AsignacionDescuentos_Descuentos");

            entity.HasOne(d => d.Empleados).WithMany(p => p.AsignacionDescuentos)
                .HasForeignKey(d => d.EmpleadosId)
                .HasConstraintName("FK_AsignacionDescuentos_Empleados");
        });

        modelBuilder.Entity<Bono>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bono__3214EC0781DB92D9");

            entity.ToTable("Bono");

            entity.Property(e => e.NombreBono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Valor).HasColumnType("decimal(8, 2)");
        });

        modelBuilder.Entity<ControlAsistencium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlA__3214EC07A766CD74");

            entity.Property(e => e.Asistencia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Dia)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Empleados).WithMany(p => p.ControlAsistencia)
                .HasForeignKey(d => d.EmpleadosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlAsistencias_Empleados");
        });

        modelBuilder.Entity<Descuento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Descuent__3214EC077DD7D9C4");

            entity.ToTable("Descuento");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Valor).HasColumnType("decimal(8, 2)");
        });

        modelBuilder.Entity<DescuentoPlanilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Descuent__3214EC0740383979");

            entity.ToTable("DescuentoPlanilla");

            entity.HasOne(d => d.AsignacionDescuento).WithMany(p => p.DescuentoPlanillas)
                .HasForeignKey(d => d.AsignacionDescuentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DescuentoPlanilla_AsignacionDescuento");
        });

        modelBuilder.Entity<DevengoPlanilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DevengoP__3214EC079A08632A");

            entity.ToTable("DevengoPlanilla");

            entity.HasOne(d => d.AsignacionBonos).WithMany(p => p.DevengoPlanillas)
                .HasForeignKey(d => d.AsignacionBonosId)
                .HasConstraintName("FK_DivingoPlanilla_AsignacionBonos");

            entity.HasOne(d => d.EmpleadoPlanilla).WithMany(p => p.DevengoPlanillas)
                .HasForeignKey(d => d.EmpleadoPlanillaId)
                .HasConstraintName("FK_DivingoPlanilla_EmpleadoPlanilla");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empleado__3214EC07D0B4CC6A");

            entity.ToTable("Empleado");

            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Dui)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DUI");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(32)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SalarioBase).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Usuario)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.JefeInmediato).WithMany(p => p.InverseJefeInmediato)
                .HasForeignKey(d => d.JefeInmediatoId)
                .HasConstraintName("FK_Empleado_Jefe");

            entity.HasOne(d => d.PuestoTrabajo).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.PuestoTrabajoId)
                .HasConstraintName("FK_Empleado_Puesto");

            entity.HasOne(d => d.TipoDeHorario).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.TipoDeHorarioId)
                .HasConstraintName("FK_Empleado_Horario");
        });

        modelBuilder.Entity<EmpleadoPlanilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empleado__3214EC0783874B59");

            entity.ToTable("EmpleadoPlanilla");

            entity.Property(e => e.LiquidoTotal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.SueldoBase).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.TotalDescuentos).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.TotalDevengos).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.TotalPagoHorasExtra).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.TotalPagoVacacion).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.ValorDeHorasExtra).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.DescuentoPlanilla).WithMany(p => p.EmpleadoPlanillas)
                .HasForeignKey(d => d.DescuentoPlanillaId)
                .HasConstraintName("FK_EmpleadoPlanilla_DescuentoPlanilla");

            entity.HasOne(d => d.Empleados).WithMany(p => p.EmpleadoPlanillas)
                .HasForeignKey(d => d.EmpleadosId)
                .HasConstraintName("FK_EmpleadoPlanilla_Empleados");

            entity.HasOne(d => d.Planilla).WithMany(p => p.EmpleadoPlanillas)
                .HasForeignKey(d => d.PlanillaId)
                .HasConstraintName("FK_EmpleadoPlanilla_Planilla");

            entity.HasOne(d => d.Vacacion).WithMany(p => p.EmpleadoPlanillas)
                .HasForeignKey(d => d.VacacionId)
                .HasConstraintName("FK_EmpleadoPlanilla_Vacacion");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Horario__3214EC071024B4F7");

            entity.ToTable("Horario");

            entity.Property(e => e.Dias)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.TipoDeHorario).WithMany(p => p.Horarios)
                .HasForeignKey(d => d.TipoDeHorarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Horarios_Tipo");
        });

        modelBuilder.Entity<Planilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Planilla__3214EC0726A1CEE0");

            entity.ToTable("Planilla");

            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.NombrePlanilla)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TotalPago).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.TipoPlanilla).WithMany(p => p.Planillas)
                .HasForeignKey(d => d.TipoPlanillaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Planilla_TipoPlanilla");
        });

        modelBuilder.Entity<PuestoTrabajo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PuestoTr__3214EC075CAB6D4A");

            entity.ToTable("PuestoTrabajo");

            entity.Property(e => e.NombrePuesto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SalarioBase).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.ValorExtra).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.ValorxHora).HasColumnType("decimal(8, 2)");
        });

        modelBuilder.Entity<TipoPlanilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoPlan__3214EC07D704A1A6");

            entity.ToTable("TipoPlanilla");

            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime");
            entity.Property(e => e.NombreTipo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TipodeHorario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipodeHo__3214EC07A8182181");

            entity.ToTable("TipodeHorario");

            entity.Property(e => e.NombreHorario)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Vacacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Vacacion__3214EC078345B13C");

            entity.ToTable("Vacacion");

            entity.Property(e => e.AnnoVacacion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DiaFin).HasColumnType("datetime");
            entity.Property(e => e.DiaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.MesVacaciones)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PagoVacaciones).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.Empleados).WithMany(p => p.Vacacions)
                .HasForeignKey(d => d.EmpleadosId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vacaciones_Empleados");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
