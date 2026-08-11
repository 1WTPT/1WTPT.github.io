/* ===== 1WantToPiglet — менеджер тем и палитр ===== */
(function(){
  var THEMES={
    light:[
      {id:"ocean",name:"Океан",dot:"#1769e0"},
      {id:"sunrise",name:"Рассвет",dot:"#ef6a2f"},
      {id:"mint",name:"Мята",dot:"#17a06b"},
      {id:"lavender",name:"Лаванда",dot:"#8352d9"},
      {id:"slate",name:"Графит",dot:"#4b5563"}
    ],
    dark:[
      {id:"midnight",name:"Полночь",dot:"#4388f7"},
      {id:"ember",name:"Уголь",dot:"#ff8552"},
      {id:"forest",name:"Лес",dot:"#3ddc84"},
      {id:"neon",name:"Неон",dot:"#d68bff"},
      {id:"graphite",name:"Сталь",dot:"#9aa5b8"}
    ]
  };
  var MODE_KEY="theme";
  var VARIANT_KEY="themeVariant";
  var root=document.documentElement;

  function currentMode(){return root.dataset.theme==="dark"?"dark":"light"}
  function defaultVariant(mode){return mode==="dark"?"midnight":"ocean"}
  function variantIds(mode){return THEMES[mode].map(function(t){return t.id})}

  function ensureVariantForMode(){
    var mode=currentMode();
    var list=variantIds(mode);
    var variant=localStorage.getItem(VARIANT_KEY);
    if(list.indexOf(variant)===-1) variant=defaultVariant(mode);
    root.dataset.variant=variant;
    return variant;
  }

  function applyVariant(id){
    var mode=currentMode();
    if(variantIds(mode).indexOf(id)===-1) return;
    root.dataset.variant=id;
    localStorage.setItem(VARIANT_KEY,id);
    renderMenu();
  }

  function setMode(mode){
    root.dataset.theme=mode;
    localStorage.setItem(MODE_KEY,mode);
    ensureVariantForMode();
    renderMenu();
  }

  var menu=null;
  function renderMenu(){
    if(!menu) return;
    var mode=currentMode();
    var active=root.dataset.variant;
    var label=document.createElement("div");
    label.className="palette-menu-label";
    label.textContent=mode==="dark"?"Тёмные темы":"Светлые темы";
    menu.innerHTML="";
    menu.appendChild(label);
    THEMES[mode].forEach(function(t){
      var btn=document.createElement("button");
      btn.type="button";
      btn.className="palette-item"+(t.id===active?" active":"");
      btn.setAttribute("role","menuitemradio");
      btn.setAttribute("aria-checked",t.id===active?"true":"false");
      var dot=document.createElement("span");
      dot.className="palette-dot";
      dot.style.background=t.dot;
      var text=document.createElement("span");
      text.textContent=t.name;
      btn.appendChild(dot);
      btn.appendChild(text);
      btn.addEventListener("click",function(){
        applyVariant(t.id);
        closeMenu();
      });
      menu.appendChild(btn);
    });
  }

  function openMenu(){menu&&menu.classList.add("open")}
  function closeMenu(){menu&&menu.classList.remove("open")}
  function toggleMenu(){menu&&menu.classList.toggle("open")}

  function wire(){
    ensureVariantForMode();
    menu=document.getElementById("paletteMenu");
    var themeBtn=document.getElementById("theme");
    var paletteBtn=document.getElementById("palette");
    if(themeBtn){
      themeBtn.onclick=function(){setMode(currentMode()==="dark"?"light":"dark")};
    }
    if(paletteBtn){
      paletteBtn.addEventListener("click",function(e){
        e.stopPropagation();
        toggleMenu();
      });
    }
    document.addEventListener("click",function(e){
      if(menu && menu.classList.contains("open") && !menu.contains(e.target) && e.target!==paletteBtn){
        closeMenu();
      }
    });
    document.addEventListener("keydown",function(e){
      if(e.key==="Escape") closeMenu();
    });
    renderMenu();
  }

  if(document.readyState==="loading"){
    document.addEventListener("DOMContentLoaded",wire);
  }else{
    wire();
  }

  window.WTPTheme={setMode:setMode,applyVariant:applyVariant,currentMode:currentMode};
})();
