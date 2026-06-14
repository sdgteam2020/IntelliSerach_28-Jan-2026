document.querySelector("form").addEventListener("submit", function (e) {
    e.preventDefault();

    const UserNameInput = document.querySelector("#UserName");
    const passwordInput = document.querySelector("#Password");

    const plainUserName = UserNameInput.value.trim();
    const plainPassword = passwordInput.value.trim();

    // 🔴 Blank validation
    if (plainUserName === "") {
        const errorMsg = document.getElementById("txterrormsg");
        errorMsg.classList.remove("d-none");
        errorMsg.innerHTML = "Username cannot be blank";
        UserNameInput.focus();
        return;
    }

    if (plainPassword === "") {
        const errorMsg = document.getElementById("txterrormsg");
        errorMsg.innerHTML = "Password cannot be blank";
        errorMsg.classList.remove("d-none");
        passwordInput.focus();
        return;
    }

    // Hide error message if validation passes
    document.getElementById("txterrormsg").classList.add("d-none");

    // 🔐 Encrypt values
    UserNameInput.value = encryptPayloadData(plainUserName);
    passwordInput.value = encryptPayloadData(plainPassword);

    // ✅ Submit form
    this.submit();
});


/* =========================================================
   ENHANCED PARTICLE BACKGROUND ANIMATION
   - Fixed: canvas resizing no longer resets the DPI transform
             every frame (this was the cause of blurry / mis-scaled
             particles in the original).
   - Added: proper resize handling (debounced).
   - Added: mouse interaction — particles gently react/connect
             to the cursor.
   - Added: smoother glow effect on particles.
   - Added: delta-time based movement so speed is consistent
             across different frame rates / devices.
   - Performance: removed repeated getComputedStyle() calls
             inside the per-particle render loop.
========================================================= */

(function () {
    const canvas = document.getElementById('canvas');
    const context = canvas.getContext('2d');

    const dpi = window.devicePixelRatio || 1;

    const couleurs = ["#3a0088", "#930077", "#e61c5d", "#ffbd39"];
    const particle_count = 70;
    const connectionDistance = 200;
    const mouseInfluenceDistance = 150;

    let cssWidth = 0;
    let cssHeight = 0;
    let particles = [];
    let mouse = { x: null, y: null, active: false };
    let lastTime = performance.now();

    // ---- Sizing -------------------------------------------------------
    function resizeCanvas() {
        cssWidth = canvas.clientWidth;
        cssHeight = canvas.clientHeight;

        canvas.width = cssWidth * dpi;
        canvas.height = cssHeight * dpi;

        // Reset transform before re-scaling so we don't compound DPI scaling
        // on repeated resizes.
        context.setTransform(1, 0, 0, 1, 0, 0);
        context.scale(dpi, dpi);
    }

    // ---- Particle -------------------------------------------------------
    function createParticle() {
        const radius = Math.round((Math.random() * 3) + 5);
        return {
            radius,
            x: Math.random() * (cssWidth - radius * 2) + radius,
            y: Math.random() * (cssHeight - radius * 2) + radius,
            color: couleurs[Math.floor(Math.random() * couleurs.length)],
            // Speed expressed in px / second for frame-rate independence
            speedx: (Math.random() * 60 - 30),
            speedy: (Math.random() * 60 - 30),
        };
    }

    function initParticles() {
        particles = [];
        for (let i = 0; i < particle_count; i++) {
            particles.push(createParticle());
        }
    }

    // ---- Drawing -------------------------------------------------------
    function drawParticle(p) {
        context.beginPath();
        context.globalCompositeOperation = 'source-over';
        context.globalAlpha = 1;

        // Soft glow
        context.shadowBlur = 8;
        context.shadowColor = p.color;
        context.fillStyle = p.color;

        context.arc(p.x, p.y, p.radius, 0, Math.PI * 2, false);
        context.fill();
        context.closePath();
        context.shadowBlur = 0; // reset so it doesn't bleed into line drawing
    }

    function drawConnections(p) {
        for (let j = 0; j < particles.length; j++) {
            const other = particles[j];
            if (other === p) continue;

            const xd = other.x - p.x;
            const yd = other.y - p.y;
            const d = Math.sqrt(xd * xd + yd * yd);

            if (d < connectionDistance) {
                context.beginPath();
                context.globalAlpha = (connectionDistance - d) / connectionDistance;
                context.globalCompositeOperation = 'destination-over';
                context.lineWidth = 1;
                context.moveTo(p.x, p.y);
                context.lineTo(other.x, other.y);
                context.strokeStyle = p.color;
                context.lineCap = "round";
                context.stroke();
                context.closePath();
            }
        }

        // Connection to mouse cursor
        if (mouse.active) {
            const xd = mouse.x - p.x;
            const yd = mouse.y - p.y;
            const d = Math.sqrt(xd * xd + yd * yd);

            if (d < mouseInfluenceDistance) {
                context.beginPath();
                context.globalAlpha = (mouseInfluenceDistance - d) / mouseInfluenceDistance;
                context.globalCompositeOperation = 'destination-over';
                context.lineWidth = 1.5;
                context.moveTo(p.x, p.y);
                context.lineTo(mouse.x, mouse.y);
                context.strokeStyle = "#ffffff";
                context.lineCap = "round";
                context.stroke();
                context.closePath();

                // Gentle attraction toward the cursor
                p.x += xd * 0.01;
                p.y += yd * 0.01;
            }
        }
    }

    function updateParticle(p, dt) {
        p.x += p.speedx * dt;
        p.y += p.speedy * dt;

        if (p.x <= p.radius || p.x >= cssWidth - p.radius) {
            p.speedx *= -1;
            p.x = Math.max(p.radius, Math.min(cssWidth - p.radius, p.x));
        }
        if (p.y <= p.radius || p.y >= cssHeight - p.radius) {
            p.speedy *= -1;
            p.y = Math.max(p.radius, Math.min(cssHeight - p.radius, p.y));
        }
    }

    // ---- Animation loop --------------------------------------------------
    function animate(now) {
        const dt = Math.min((now - lastTime) / 1000, 0.05); // clamp big gaps
        lastTime = now;

        context.clearRect(0, 0, cssWidth, cssHeight);

        for (let i = 0; i < particles.length; i++) {
            const p = particles[i];
            updateParticle(p, dt);
            drawConnections(p);
            drawParticle(p);
        }

        requestAnimationFrame(animate);
    }

    // ---- Events -------------------------------------------------------
    let resizeTimeout;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function () {
            resizeCanvas();
        }, 150);
    });

    canvas.addEventListener('mousemove', function (e) {
        const rect = canvas.getBoundingClientRect();
        mouse.x = e.clientX - rect.left;
        mouse.y = e.clientY - rect.top;
        mouse.active = true;
    });

    canvas.addEventListener('mouseleave', function () {
        mouse.active = false;
    });

    // ---- Init -------------------------------------------------------
    resizeCanvas();
    initParticles();
    requestAnimationFrame(animate);
})();