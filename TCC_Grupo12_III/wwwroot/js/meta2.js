(() => {
 const toggle=document.querySelector('.ec-menu'),nav=document.getElementById('navegacao');
 if(toggle&&nav){
 const close=()=>{nav.classList.remove('is-open');toggle.setAttribute('aria-expanded','false');};
 toggle.addEventListener('click',()=>{const open=nav.classList.toggle('is-open');toggle.setAttribute('aria-expanded',String(open));});
 document.addEventListener('keydown',e=>{if(e.key==='Escape'){close();toggle.focus();}});
 document.addEventListener('click',e=>{if(!e.target.closest('.ec-header'))close();});
 }
 document.querySelectorAll('form[data-confirm]').forEach(form=>form.addEventListener('submit',e=>{if(!window.confirm(form.dataset.confirm))e.preventDefault();}));
 document.querySelectorAll('form[data-submit-once]').forEach(form=>form.addEventListener('submit',()=>{if(form.checkValidity())form.querySelectorAll('button[type="submit"]').forEach(b=>{b.disabled=true;b.textContent='Salvando resposta…';});}));
 document.querySelectorAll('img[data-fallback]').forEach(img=>img.addEventListener('error',()=>{img.hidden=true;}));
})();
document.querySelectorAll('.ec-history table').forEach(table=>{
 const labels=[...table.querySelectorAll('thead th')].map(h=>h.textContent.trim());
 if(!labels.length)return;
 table.querySelectorAll('tbody tr').forEach(row=>[...row.cells].forEach((cell,i)=>cell.dataset.label=labels[i]||''));
 table.classList.add('ec-table-stacked');
});