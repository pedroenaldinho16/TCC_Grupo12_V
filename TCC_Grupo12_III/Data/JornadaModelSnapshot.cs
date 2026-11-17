using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Data;
public static class JornadaModelSnapshot {
 public static void Configurar(ModelBuilder b){
 b.Entity<JornadaPergunta>(e=>{
 e.ToTable("JornadaPerguntas");e.HasKey(x=>x.Id);e.Property(x=>x.Id).ValueGeneratedOnAdd();e.Property(x=>x.Enunciado).IsRequired().HasMaxLength(1500);
 e.Property(x=>x.OpcaoA).IsRequired();e.Property(x=>x.OpcaoB).IsRequired();e.Property(x=>x.OpcaoC).IsRequired();e.Property(x=>x.OpcaoD).IsRequired();
 e.Property(x=>x.Explicacao).IsRequired();e.Property(x=>x.Fonte).IsRequired();
 });
 b.Entity<JornadaTentativa>(e=>{e.ToTable("JornadaTentativas");e.HasKey(x=>x.Id);e.Property(x=>x.Id).ValueGeneratedOnAdd();
 e.Property(x=>x.QuestoesJson).IsRequired();e.Property(x=>x.RespostasJson).IsRequired();
 e.HasOne(x=>x.Usuario).WithMany().HasForeignKey(x=>x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
 e.HasIndex(x=>new{x.UsuarioId,x.Desafio});e.HasIndex(x=>new{x.UsuarioId,x.Desafio},"IX_JornadaTentativas_EmAndamento").IsUnique().HasFilter("ConcluidaEm IS NULL");
 });
 }
}
