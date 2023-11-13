using APIImportacionComprobantes.BO;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Importacion_Comprobantes.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class comprobantesController : ControllerBase
    {


        [HttpPost("AltaCobros")]
        public void AltaCobros([FromBody] Cobro oCobro)
        {
            int hola = 0;

        }


        [HttpPost("AltaPedidos")]
        public void AltaPedidos([FromBody] Pedido oPedido)
        {
            int hola = 0;

        }

        // GET: api/<comprobantesController>
        /*  [HttpGet]
          public IEnumerable<string> Get()
          {
              return new string[] { "value1", "value2" };
          }

          // GET api/<comprobantesController>/5
          [HttpGet("{id}")]
          public string Get(int id)
          {
              return "value";
          }

          // POST api/<comprobantesController>
          [HttpPost]
          public void Post([FromBody] string value)
          {
          }

          // PUT api/<comprobantesController>/5
          [HttpPut("{id}")]
          public void Put(int id, [FromBody] string value)
          {
          }

          // DELETE api/<comprobantesController>/5
          [HttpDelete("{id}")]
          public void Delete(int id)
          {
          }*/
    }
}
