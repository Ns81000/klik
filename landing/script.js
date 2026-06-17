/* ============================================================
   KLIK LANDING PAGE — Interactivity
   Typing simulation, scroll reveals, nav behavior, mobile menu
   ============================================================ */

(function () {
  "use strict";

  // ========================= //
  // 1. TYPING SIMULATION      //
  // ========================= //

  const messages = [
    "Haan ab bol",
    "Kitni baar bhejun yaar 😭",
    "Bhai dekh raha hai ya so raha hai?",
    "Message chala gaya, tu ja sakta hai 🙃",
    "Aur ek baar? Seriously?",
  ];

  const typedTextEl = document.getElementById("typed-text");
  const counterEl = document.getElementById("mock-counter");
  const timerEl = document.getElementById("mock-timer");
  const enterKeyEl = document.getElementById("mock-enter-key");

  let messageIndex = 0;
  let charIndex = 0;
  let sendCount = 0;
  let elapsedSeconds = 0;
  let timerInterval = null;
  let isTyping = false;

  function formatTime(totalSeconds) {
    const m = Math.floor(totalSeconds / 60)
      .toString()
      .padStart(2, "0");
    const s = (totalSeconds % 60).toString().padStart(2, "0");
    return m + ":" + s;
  }

  function startTimer() {
    if (timerInterval) return;
    timerInterval = setInterval(function () {
      elapsedSeconds++;
      if (timerEl) timerEl.textContent = formatTime(elapsedSeconds);
    }, 1000);
  }

  function typeNextChar() {
    if (!typedTextEl) return;
    isTyping = true;

    const message = messages[messageIndex];
    if (charIndex < message.length) {
      typedTextEl.textContent = message.substring(0, charIndex + 1);
      charIndex++;
      const delay = 45 + Math.random() * 55;
      setTimeout(typeNextChar, delay);
    } else {
      setTimeout(pressEnter, 600);
    }
  }

  function pressEnter() {
    if (!enterKeyEl) return;

    enterKeyEl.classList.add("keycap--pressed");

    setTimeout(function () {
      enterKeyEl.classList.remove("keycap--pressed");

      sendCount++;
      if (counterEl) counterEl.textContent = sendCount;

      setTimeout(function () {
        if (typedTextEl) typedTextEl.textContent = "";
        charIndex = 0;
        messageIndex = (messageIndex + 1) % messages.length;
        setTimeout(typeNextChar, 600);
      }, 800);
    }, 180);
  }

  function initTypingSimulation() {
    if (!typedTextEl) return;

    const prefersReducedMotion = window.matchMedia(
      "(prefers-reduced-motion: reduce)"
    ).matches;

    if (prefersReducedMotion) {
      typedTextEl.textContent = messages[0];
      if (counterEl) counterEl.textContent = "12";
      if (timerEl) timerEl.textContent = "01:00";
      return;
    }

    setTimeout(function () {
      startTimer();
      typeNextChar();
    }, 1200);
  }

  // ========================= //
  // 2. SCROLL REVEAL           //
  // ========================= //

  function initScrollReveal() {
    const revealElements = document.querySelectorAll(".reveal");
    if (!revealElements.length) return;

    const prefersReducedMotion = window.matchMedia(
      "(prefers-reduced-motion: reduce)"
    ).matches;

    if (prefersReducedMotion) {
      revealElements.forEach(function (el) {
        el.classList.add("reveal--visible");
      });
      return;
    }

    const observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add("reveal--visible");
            observer.unobserve(entry.target);
          }
        });
      },
      {
        threshold: 0.15,
        rootMargin: "-40px 0px",
      }
    );

    revealElements.forEach(function (el) {
      observer.observe(el);
    });
  }

  // ========================= //
  // 3. NAV SCROLL BEHAVIOR    //
  // ========================= //

  function initNavScroll() {
    const nav = document.getElementById("main-nav");
    if (!nav) return;

    let ticking = false;

    // Set initial state immediately without transition
    if (window.scrollY > 20) {
      nav.classList.add("nav--scrolled");
    }

    window.addEventListener(
      "scroll",
      function () {
        if (!ticking) {
          requestAnimationFrame(function () {
            if (window.scrollY > 20) {
              nav.classList.remove("nav--no-transition");
              nav.classList.add("nav--scrolled");
            } else {
              // Kill transition before removing scrolled so there's no fade flash
              nav.classList.add("nav--no-transition");
              nav.classList.remove("nav--scrolled");
            }
            ticking = false;
          });
          ticking = true;
        }
      },
      { passive: true }
    );
  }

  // ========================= //
  // 4. MOBILE MENU             //
  // ========================= //

  function initMobileMenu() {
    const toggle = document.getElementById("nav-toggle");
    const drawer = document.getElementById("nav-drawer");
    if (!toggle || !drawer) return;

    var scrollY = 0;

    function openDrawer() {
      scrollY = window.scrollY;
      document.body.style.position = "fixed";
      document.body.style.top = "-" + scrollY + "px";
      document.body.style.left = "0";
      document.body.style.right = "0";
      requestAnimationFrame(function () {
        toggle.classList.add("nav__hamburger--open");
        drawer.classList.add("nav__drawer--open");
        toggle.setAttribute("aria-expanded", "true");
      });
    }

    function closeDrawer(targetEl) {
      toggle.classList.remove("nav__hamburger--open");
      drawer.classList.remove("nav__drawer--open");
      toggle.setAttribute("aria-expanded", "false");
      document.body.style.position = "";
      document.body.style.top = "";
      document.body.style.left = "";
      document.body.style.right = "";

      // restore scroll position instantly, without smooth-scroll interference
      document.documentElement.style.scrollBehavior = "auto";
      document.documentElement.scrollTop = scrollY;

      if (targetEl) {
        // let the restore paint first, then smooth scroll to target
        requestAnimationFrame(function () {
          document.documentElement.style.scrollBehavior = "";
          targetEl.scrollIntoView({ behavior: "smooth", block: "start" });
        });
      } else {
        requestAnimationFrame(function () {
          document.documentElement.style.scrollBehavior = "";
        });
      }
    }

    toggle.addEventListener("click", function () {
      if (toggle.classList.contains("nav__hamburger--open")) {
        closeDrawer(null);
      } else {
        openDrawer();
      }
    });

    var drawerLinks = drawer.querySelectorAll("a");
    drawerLinks.forEach(function (link) {
      link.addEventListener("click", function (e) {
        var href = this.getAttribute("href");
        var targetEl = (href && href !== "#") ? document.querySelector(href) : null;
        if (targetEl) e.preventDefault();
        closeDrawer(targetEl);
      });
    });
  }

  // ========================= //
  // 5. SMOOTH SCROLL           //
  // ========================= //

  function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
      // skip drawer links — handled by initMobileMenu
      if (anchor.closest("#nav-drawer")) return;

      anchor.addEventListener("click", function (e) {
        const targetId = this.getAttribute("href");
        if (targetId === "#") return;

        const targetEl = document.querySelector(targetId);
        if (targetEl) {
          e.preventDefault();
          targetEl.scrollIntoView({ behavior: "smooth", block: "start" });
        }
      });
    });
  }

  // ========================= //
  // 6. FLUID BACKGROUND        //
  // ========================= //

  function initFluidBg() {
    const canvas = document.getElementById("fluid-bg");
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    // Respect reduced motion
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
      canvas.style.display = "none";
      return;
    }

    var W = 0, H = 0, dpr = Math.min(window.devicePixelRatio || 1, 2);
    var scrollProgress = 0;
    var targetScroll = 0;
    var rafId = null;

    function resize() {
      W = window.innerWidth;
      H = window.innerHeight;
      canvas.width = Math.round(W * dpr);
      canvas.height = Math.round(H * dpr);
      canvas.style.width = W + "px";
      canvas.style.height = H + "px";
      ctx.scale(dpr, dpr);
    }

    // Blobs — each has position, size, color, speed, orbit radius
    var blobs = [
      { x: 0.15, y: 0.20, r: 0.55, ox: 0.08, oy: 0.06, speed: 0.00018, phase: 0.0,  color: "rgba(84, 116, 180, 0.13)"  },
      { x: 0.80, y: 0.15, r: 0.45, ox: 0.07, oy: 0.09, speed: 0.00013, phase: 1.8,  color: "rgba(122, 162, 247, 0.09)" },
      { x: 0.50, y: 0.55, r: 0.60, ox: 0.10, oy: 0.05, speed: 0.00015, phase: 3.4,  color: "rgba(84, 116, 180, 0.08)"  },
      { x: 0.10, y: 0.75, r: 0.40, ox: 0.06, oy: 0.08, speed: 0.00020, phase: 0.9,  color: "rgba(122, 162, 247, 0.07)" },
      { x: 0.85, y: 0.70, r: 0.50, ox: 0.09, oy: 0.07, speed: 0.00016, phase: 2.5,  color: "rgba(84, 116, 180, 0.10)"  },
      { x: 0.40, y: 0.90, r: 0.38, ox: 0.05, oy: 0.10, speed: 0.00022, phase: 4.1,  color: "rgba(122, 162, 247, 0.06)" },
    ];

    var lastTime = 0;

    function draw(now) {
      rafId = requestAnimationFrame(draw);

      var dt = now - lastTime;
      lastTime = now;
      if (dt > 100) dt = 100; // cap on tab resume

      // Smooth scroll interpolation
      scrollProgress += (targetScroll - scrollProgress) * 0.04;

      ctx.clearRect(0, 0, W, H);

      for (var i = 0; i < blobs.length; i++) {
        var b = blobs[i];
        b.phase += b.speed * dt;

        // Base position + gentle orbit + scroll drift
        var scrollDrift = scrollProgress * 0.3;
        var cx = (b.x + Math.sin(b.phase) * b.ox + scrollDrift * (i % 2 === 0 ? 0.08 : -0.06)) * W;
        var cy = (b.y + Math.cos(b.phase * 0.7) * b.oy + scrollProgress * 0.15 * (i * 0.1)) * H;
        var radius = b.r * Math.max(W, H) * 0.6;

        var grad = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius);
        grad.addColorStop(0, b.color);
        grad.addColorStop(1, "transparent");

        ctx.fillStyle = grad;
        ctx.beginPath();
        ctx.arc(cx, cy, radius, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    function onScroll() {
      var docH = document.documentElement.scrollHeight - window.innerHeight;
      targetScroll = docH > 0 ? window.scrollY / docH : 0;
    }

    var resizeTimer;
    window.addEventListener("resize", function () {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(function () {
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        resize();
      }, 150);
    }, { passive: true });

    window.addEventListener("scroll", onScroll, { passive: true });

    resize();
    rafId = requestAnimationFrame(draw);
  }

  // ========================= //
  // INIT                       //
  // ========================= //

  document.addEventListener("DOMContentLoaded", function () {
    initTypingSimulation();
    initScrollReveal();
    initNavScroll();
    initMobileMenu();
    initSmoothScroll();
    initFluidBg();
  });
})();
