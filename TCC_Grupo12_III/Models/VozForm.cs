using System.ComponentModel.DataAnnotations;
namespace TCC_Grupo12_III.Models;
public class VozForm{
 [Required,StringLength(200)]public string Titulo{get;set;}="";
 [Required,StringLength(1000)]public string Resumo{get;set;}="";
 [Required,StringLength(20000)]public string Conteudo{get;set;}="";
 [Required,StringLength(2000)]public string Referencias{get;set;}="";
}