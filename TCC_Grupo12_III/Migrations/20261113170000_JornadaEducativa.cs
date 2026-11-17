using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TCC_Grupo12_III.Data;
namespace TCC_Grupo12_III.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261113170000_JornadaEducativa")]
public partial class JornadaEducativa:Migration {
 protected override void Up(MigrationBuilder m) {
m.AddColumn<string>("ImportanciaEcologica","Especies",nullable:false,defaultValue:"");
m.AddColumn<string>("AcoesPreservacao","Especies",nullable:false,defaultValue:"");
m.AddColumn<string>("FonteCientifica","Especies",nullable:false,defaultValue:"");
m.AddColumn<string>("ConservacaoGlobal","Especies",nullable:false,defaultValue:"");
m.AddColumn<string>("ConservacaoNacional","Especies",nullable:false,defaultValue:"");
m.AddColumn<string>("ConservacaoRegional","Especies",nullable:false,defaultValue:"");
m.AddColumn<bool>("Aprovado","Avistamentos",nullable:false,defaultValue:false);
 m.CreateTable(name:"JornadaPerguntas",columns:t=>new {
 Id=t.Column<int>(nullable:false).Annotation("Sqlite:Autoincrement",true),Desafio=t.Column<int>(nullable:false),
 Enunciado=t.Column<string>(maxLength:1500,nullable:false),OpcaoA=t.Column<string>(nullable:false),OpcaoB=t.Column<string>(nullable:false),
 OpcaoC=t.Column<string>(nullable:false),OpcaoD=t.Column<string>(nullable:false),Correta=t.Column<int>(nullable:false),
 Explicacao=t.Column<string>(nullable:false),Fonte=t.Column<string>(nullable:false),ImagemUrl=t.Column<string>(nullable:true),Ativa=t.Column<bool>(nullable:false)
 },constraints:t=>t.PrimaryKey("PK_JornadaPerguntas",x=>x.Id));
 m.CreateTable(name:"JornadaTentativas",columns:t=>new {
 Id=t.Column<int>(nullable:false).Annotation("Sqlite:Autoincrement",true),UsuarioId=t.Column<int>(nullable:false),Desafio=t.Column<int>(nullable:false),
 IniciadaEm=t.Column<DateTime>(nullable:false),ConcluidaEm=t.Column<DateTime>(nullable:true),Pontos=t.Column<int>(nullable:false),
 Total=t.Column<int>(nullable:false),Aprovada=t.Column<bool>(nullable:false),QuestoesJson=t.Column<string>(nullable:false),RespostasJson=t.Column<string>(nullable:false)
 },constraints:t=>{t.PrimaryKey("PK_JornadaTentativas",x=>x.Id);t.ForeignKey("FK_JornadaTentativas_Usuarios_UsuarioId",x=>x.UsuarioId,"Usuarios","Id",onDelete:ReferentialAction.Restrict);});
 m.CreateIndex("IX_JornadaTentativas_UsuarioId_Desafio","JornadaTentativas",new[]{"UsuarioId","Desafio"});
 m.CreateIndex("IX_JornadaTentativas_EmAndamento","JornadaTentativas",new[]{"UsuarioId","Desafio"},unique:true,filter:"ConcluidaEm IS NULL");
 }
 protected override void Down(MigrationBuilder m) {
m.DropColumn("ImportanciaEcologica","Especies");
m.DropColumn("AcoesPreservacao","Especies");
m.DropColumn("FonteCientifica","Especies");
m.DropColumn("ConservacaoGlobal","Especies");
m.DropColumn("ConservacaoNacional","Especies");
m.DropColumn("ConservacaoRegional","Especies");
m.DropColumn("Aprovado","Avistamentos");m.DropTable("JornadaTentativas");m.DropTable("JornadaPerguntas");}

}
