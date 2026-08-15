/* ===== 1WantToPiglet — общий скрипт интерфейса ===== */
(function(){
  "use strict";

  /* ---------- тема (светлая/тёмная), без набора скинов ---------- */
  var root=document.documentElement;
  function currentMode(){return root.dataset.theme==="light"?"light":"dark"}
  function setMode(mode){
    root.dataset.theme=mode;
    try{localStorage.setItem("wtp_theme",mode)}catch(e){}
  }
  window.WTPTheme={setMode:setMode,currentMode:currentMode};

  function wireChrome(){
    var themeBtn=document.getElementById("theme");
    if(themeBtn) themeBtn.addEventListener("click",function(){
      setMode(currentMode()==="dark"?"light":"dark");
    });

    var drawer=document.getElementById("drawer");
    var overlay=document.getElementById("overlay");
    var closeDrawer=function(){
      drawer&&drawer.classList.remove("open");
      overlay&&overlay.classList.remove("show");
    };
    var menuBtn=document.getElementById("menuBtn");
    menuBtn&&menuBtn.addEventListener("click",function(){
      drawer&&drawer.classList.add("open");
      overlay&&overlay.classList.add("show");
    });
    var closeBtn=document.getElementById("closeDrawer");
    closeBtn&&closeBtn.addEventListener("click",closeDrawer);
    overlay&&overlay.addEventListener("click",closeDrawer);
    document.addEventListener("keydown",function(e){ if(e.key==="Escape") closeDrawer(); });

    var logoutBtn=document.getElementById("logoutBtn");
    logoutBtn&&logoutBtn.addEventListener("click",function(){ window.WTPAuth&&WTPAuth.logout(); });

    var yearEl=document.getElementById("year");
    if(yearEl) yearEl.textContent=new Date().getFullYear();
  }

  /* ---------- тосты и копирование ---------- */
  var toast=null;
  function notify(text){
    if(!toast) toast=document.getElementById("toast");
    if(!toast) return;
    toast.textContent=text;
    toast.classList.add("show");
    clearTimeout(window.__wtpToastTimer);
    window.__wtpToastTimer=setTimeout(function(){toast.classList.remove("show")},1500);
  }
  async function copyText(text){
    try{ await navigator.clipboard.writeText(text); return true; }
    catch(e){
      var area=document.createElement("textarea");
      area.value=text;area.style.position="fixed";area.style.opacity="0";
      document.body.appendChild(area);area.select();
      var ok=document.execCommand("copy");area.remove();return ok;
    }
  }
  function wireCopy(){
    document.querySelectorAll("[data-copy]").forEach(function(btn){
      btn.addEventListener("click",async function(){
        var ok=await copyText(btn.dataset.copy);
        notify(ok?"Скопировано":"Ошибка копирования");
      });
    });
  }

  /* ---------- сигнатурный эффект: латунный наклон + блик ----------
     Использует jQuery Plate для физического 3D-наклона панели вслед за
     курсором, плюс лёгкий «блик» (radial-gradient), который скользит по
     поверхности — как отражение света на полированной латуни. */
  function wireTilt(){
    var reduce=window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if(reduce) return;
    var els=document.querySelectorAll(".panel, .card, .module, .feature, .login-card");
    if(!els.length) return;

    if(window.jQuery && jQuery.fn.plate){
      jQuery(els).plate({perspective:900,maxRotation:3.2,animationDuration:260});
    }

    els.forEach(function(el){
      el.addEventListener("mousemove",function(e){
        var r=el.getBoundingClientRect();
        el.style.setProperty("--mx",(e.clientX-r.left)+"px");
        el.style.setProperty("--my",(e.clientY-r.top)+"px");
        el.classList.add("tilt-active");
      });
      el.addEventListener("mouseleave",function(){
        el.classList.remove("tilt-active");
      });
    });
  }

  function boot(){
    wireChrome();
    wireCopy();
    wireTilt();
  }

  if(document.readyState==="loading") document.addEventListener("DOMContentLoaded",boot);
  else boot();
})();
