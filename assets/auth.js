/* ===== 1WantToPiglet — вход по паролю с привязкой к IP =====
   Пароль хранится только в виде SHA-256 хэша, сам пароль нигде
   на сайте не сохраняется и не может быть получен из исходного кода.
   Сессия привязывается к публичному IP устройства: если запрос идёт
   с того же IP, повторный ввод пароля не требуется 5 дней. При смене
   IP пароль запрашивается заново. */
(function(window){
  var AUTH_KEY="wtp_auth_v1";
  var TTL_MS=5*24*60*60*1000; // 5 дней
  var PASS_HASH="d8ee3ee22f2100a6db74feec5877e28bd98d38bdeb6f6a3b4a1bca59f267b9c5";
  var IP_ENDPOINTS=[
    "https://api.ipify.org?format=json",
    "https://api64.ipify.org?format=json"
  ];
  var FAIL_KEY="wtp_auth_fail_v1";
  var MAX_ATTEMPTS=5;
  var LOCKOUT_MS=5*60*1000; // 5 минут

  function bufToHex(buf){
    var bytes=new Uint8Array(buf),hex="";
    for(var i=0;i<bytes.length;i++) hex+=bytes[i].toString(16).padStart(2,"0");
    return hex;
  }

  function sha256Hex(str){
    if(!(window.crypto && window.crypto.subtle)){
      return Promise.reject(new Error("Web Crypto недоступен"));
    }
    var data=new TextEncoder().encode(str);
    return window.crypto.subtle.digest("SHA-256",data).then(bufToHex);
  }

  function fetchIP(){
    function tryNext(i){
      if(i>=IP_ENDPOINTS.length) return Promise.resolve(null);
      return fetch(IP_ENDPOINTS[i],{cache:"no-store"})
        .then(function(r){ if(!r.ok) throw 0; return r.json(); })
        .then(function(d){ return (d && d.ip) ? d.ip : tryNext(i+1); })
        .catch(function(){ return tryNext(i+1); });
    }
    return tryNext(0);
  }

  function readAuth(){
    try{
      var raw=localStorage.getItem(AUTH_KEY);
      if(!raw) return null;
      var obj=JSON.parse(raw);
      if(!obj || !obj.ip || !obj.expires) return null;
      return obj;
    }catch(e){ return null; }
  }

  function writeAuth(ip){
    localStorage.setItem(AUTH_KEY,JSON.stringify({ip:ip,expires:Date.now()+TTL_MS}));
  }

  function clearAuth(){
    localStorage.removeItem(AUTH_KEY);
  }

  function reveal(){
    document.documentElement.classList.remove("auth-pending");
  }

  function safeNext(path){
    if(typeof path==="string" && path.indexOf("/")===0 && path.indexOf("//")!==0) return path;
    return "/home/";
  }

  function goToLogin(){
    var next=encodeURIComponent(location.pathname+location.search);
    location.replace("/login/?next="+next);
  }

  /* Вызывается на защищённых страницах: скрывает контент, пока не
     подтверждена сессия по IP, иначе отправляет на экран логина. */
  function guard(){
    var auth=readAuth();
    if(!auth || Date.now()>auth.expires){
      goToLogin();
      return;
    }
    fetchIP().then(function(ip){
      if(!ip || ip!==auth.ip){
        clearAuth();
        goToLogin();
        return;
      }
      reveal();
    });
  }

  function checkPassword(entered){
    return sha256Hex(entered).then(function(hash){ return hash===PASS_HASH; });
  }

  /* ===== Защита от подбора пароля: 5 неверных попыток блокируют
     форму входа на 5 минут. Счётчик хранится в localStorage. ===== */
  function readFails(){
    try{
      var raw=localStorage.getItem(FAIL_KEY);
      if(!raw) return {count:0,lockUntil:0};
      var obj=JSON.parse(raw);
      if(!obj || typeof obj.count!=="number") return {count:0,lockUntil:0};
      return {count:obj.count||0,lockUntil:obj.lockUntil||0};
    }catch(e){ return {count:0,lockUntil:0}; }
  }

  function writeFails(state){
    try{ localStorage.setItem(FAIL_KEY,JSON.stringify(state)); }catch(e){}
  }

  /* Возвращает {locked, remainingMs, attemptsLeft}. Если время блокировки
     истекло — автоматически сбрасывает счётчик попыток. */
  function lockState(){
    var state=readFails();
    if(state.lockUntil && Date.now()>=state.lockUntil){
      state={count:0,lockUntil:0};
      writeFails(state);
    }
    var locked=!!(state.lockUntil && Date.now()<state.lockUntil);
    return {
      locked:locked,
      remainingMs:locked?(state.lockUntil-Date.now()):0,
      attemptsLeft:Math.max(0,MAX_ATTEMPTS-state.count)
    };
  }

  function registerFailure(){
    var state=readFails();
    if(state.lockUntil && Date.now()<state.lockUntil) return lockState();
    state.count=(state.count||0)+1;
    if(state.count>=MAX_ATTEMPTS){
      state.lockUntil=Date.now()+LOCKOUT_MS;
    }
    writeFails(state);
    return lockState();
  }

  function registerSuccess(){
    writeFails({count:0,lockUntil:0});
  }

  /* Успешный вход: сохраняет привязку к текущему IP на 5 дней.
     Возвращает объект {ok, locked, remainingMs, attemptsLeft}. */
  function login(entered){
    var pre=lockState();
    if(pre.locked) return Promise.resolve(pre);
    return checkPassword(entered).then(function(ok){
      if(!ok){
        var state=registerFailure();
        state.ok=false;
        return state;
      }
      registerSuccess();
      return fetchIP().then(function(ip){
        writeAuth(ip || "unknown");
        return {ok:true,locked:false,remainingMs:0,attemptsLeft:MAX_ATTEMPTS};
      });
    });
  }

  function logout(){
    clearAuth();
    location.href="/login/";
  }

  window.WTPAuth={
    guard:guard,
    login:login,
    logout:logout,
    readAuth:readAuth,
    clearAuth:clearAuth,
    reveal:reveal,
    safeNext:safeNext,
    fetchIP:fetchIP,
    lockState:lockState
  };
})(window);
