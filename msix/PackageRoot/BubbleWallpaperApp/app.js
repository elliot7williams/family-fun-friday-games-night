var canvas = document.getElementById("bubbleCanvas");
var ctx = canvas.getContext("2d");
var paletteSelect = document.getElementById("palette");
var densityInput = document.getElementById("density");
var speedInput = document.getElementById("speed");
var sizeInput = document.getElementById("size");
var shuffleButton = document.getElementById("shuffleButton");
var soundButton = document.getElementById("soundButton");
var textButton = document.getElementById("textButton");
var downloadButton = document.getElementById("downloadButton");
var fullscreenButton = document.getElementById("fullscreenButton");
var wallpaperTitle = document.querySelector(".wallpaper-title");

var palettes = {
  candy: {
    background: ["#071017", "#173040", "#281638"],
    bubbles: ["#ee7752", "#e73c7e", "#23a6d5", "#23d5ab", "#ffdfd3", "#b28dff"]
  },
  lagoon: {
    background: ["#03191d", "#063c4a", "#102d40"],
    bubbles: ["#7ef9ff", "#34d399", "#22d3ee", "#a7f3d0", "#f8fafc"]
  },
  sunset: {
    background: ["#1b1024", "#4b1632", "#7b2d26"],
    bubbles: ["#fb7185", "#f97316", "#fde68a", "#c084fc", "#fdf2f8"]
  },
  midnight: {
    background: ["#020617", "#0f172a", "#172554"],
    bubbles: ["#93c5fd", "#a5b4fc", "#67e8f9", "#f0f9ff", "#c4b5fd"]
  }
};

var bubbles = [];
var particles = [];
var soundEnabled = true;
var textVisible = true;
var lastFrame = 0;
var pixelRatio = 1;

function randomBetween(min, max) {
  return min + Math.random() * (max - min);
}

function currentPalette() {
  return palettes[paletteSelect.value] || palettes.candy;
}

function resizeCanvas() {
  var rect = canvas.getBoundingClientRect();
  pixelRatio = Math.min(window.devicePixelRatio || 1, 2);
  canvas.width = Math.max(1, Math.floor(rect.width * pixelRatio));
  canvas.height = Math.max(1, Math.floor(rect.height * pixelRatio));
  ctx.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
}

function makeBubble(resetY) {
  var rect = canvas.getBoundingClientRect();
  var baseSize = Number(sizeInput.value);
  var radius = randomBetween(baseSize * 0.28, baseSize * 0.72);

  return {
    x: randomBetween(-radius, rect.width + radius),
    y: resetY ? rect.height + radius + randomBetween(0, rect.height * 0.65) : randomBetween(-radius, rect.height + radius),
    radius: radius,
    drift: randomBetween(0.55, 1.45),
    phase: randomBetween(0, Math.PI * 2),
    speed: randomBetween(0.25, 0.72),
    alpha: randomBetween(0.38, 0.72),
    color: currentPalette().bubbles[Math.floor(Math.random() * currentPalette().bubbles.length)]
  };
}

function rebuildBubbles() {
  var count = Number(densityInput.value);
  bubbles = [];

  for (var i = 0; i < count; i++) {
    bubbles.push(makeBubble(false));
  }
}

function drawBackground(width, height) {
  var palette = currentPalette();
  var gradient = ctx.createLinearGradient(0, 0, width, height);
  gradient.addColorStop(0, palette.background[0]);
  gradient.addColorStop(0.48, palette.background[1]);
  gradient.addColorStop(1, palette.background[2]);
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, width, height);
}

function drawBubble(bubble) {
  var highlightX = bubble.x - bubble.radius * 0.32;
  var highlightY = bubble.y - bubble.radius * 0.36;
  var gradient = ctx.createRadialGradient(highlightX, highlightY, bubble.radius * 0.08, bubble.x, bubble.y, bubble.radius);

  gradient.addColorStop(0, "rgba(255, 255, 255, 0.96)");
  gradient.addColorStop(0.18, bubble.color);
  gradient.addColorStop(0.68, "rgba(255, 255, 255, 0.2)");
  gradient.addColorStop(1, "rgba(255, 255, 255, 0.06)");

  ctx.save();
  ctx.globalAlpha = bubble.alpha;
  ctx.fillStyle = gradient;
  ctx.beginPath();
  ctx.arc(bubble.x, bubble.y, bubble.radius, 0, Math.PI * 2);
  ctx.fill();
  ctx.lineWidth = Math.max(1, bubble.radius * 0.035);
  ctx.strokeStyle = "rgba(255, 255, 255, 0.42)";
  ctx.stroke();
  ctx.restore();
}

function drawParticles(delta) {
  for (var i = particles.length - 1; i >= 0; i--) {
    var particle = particles[i];
    particle.life -= delta;
    particle.x += particle.vx * delta * 0.06;
    particle.y += particle.vy * delta * 0.06;

    if (particle.life <= 0) {
      particles.splice(i, 1);
      continue;
    }

    ctx.save();
    ctx.globalAlpha = Math.max(0, particle.life / particle.maxLife);
    ctx.fillStyle = particle.color;
    ctx.beginPath();
    ctx.arc(particle.x, particle.y, particle.radius, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }
}

function frame(time) {
  var rect = canvas.getBoundingClientRect();
  var delta = Math.min(40, time - lastFrame || 16);
  var speedScale = Number(speedInput.value) / 100;

  lastFrame = time;
  drawBackground(rect.width, rect.height);

  for (var i = 0; i < bubbles.length; i++) {
    var bubble = bubbles[i];
    bubble.phase += delta * 0.0014 * bubble.drift;
    bubble.y -= bubble.speed * speedScale * delta * 0.08;
    bubble.x += Math.sin(bubble.phase) * bubble.drift * delta * 0.035;

    if (bubble.y + bubble.radius < -20) {
      bubbles[i] = makeBubble(true);
      bubbles[i].y = rect.height + bubble.radius;
      bubble = bubbles[i];
    }

    drawBubble(bubble);
  }

  drawParticles(delta);
  requestAnimationFrame(frame);
}

function playPopSound() {
  var AudioContext = window.AudioContext || window.webkitAudioContext;

  if (!soundEnabled || !AudioContext) {
    return;
  }

  var audioContext = new AudioContext();
  var now = audioContext.currentTime;
  var oscillator = audioContext.createOscillator();
  var gain = audioContext.createGain();

  oscillator.type = "sine";
  oscillator.frequency.setValueAtTime(780, now);
  oscillator.frequency.exponentialRampToValueAtTime(170, now + 0.09);
  gain.gain.setValueAtTime(0.001, now);
  gain.gain.exponentialRampToValueAtTime(0.16, now + 0.01);
  gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);
  oscillator.connect(gain);
  gain.connect(audioContext.destination);
  oscillator.start(now);
  oscillator.stop(now + 0.13);

  window.setTimeout(function () {
    audioContext.close();
  }, 180);
}

function popBubbleAt(x, y) {
  for (var i = bubbles.length - 1; i >= 0; i--) {
    var bubble = bubbles[i];
    var dx = x - bubble.x;
    var dy = y - bubble.y;

    if ((dx * dx) + (dy * dy) <= bubble.radius * bubble.radius) {
      playPopSound();

      for (var p = 0; p < 14; p++) {
        particles.push({
          x: bubble.x + randomBetween(-bubble.radius * 0.2, bubble.radius * 0.2),
          y: bubble.y + randomBetween(-bubble.radius * 0.2, bubble.radius * 0.2),
          vx: randomBetween(-bubble.radius, bubble.radius),
          vy: randomBetween(-bubble.radius, bubble.radius),
          radius: randomBetween(2, 5),
          life: randomBetween(220, 360),
          maxLife: 360,
          color: bubble.color
        });
      }

      bubbles.splice(i, 1);
      bubbles.push(makeBubble(true));
      return;
    }
  }
}

function canvasPoint(event) {
  var rect = canvas.getBoundingClientRect();
  var source = event.touches && event.touches.length ? event.touches[0] : event;

  return {
    x: source.clientX - rect.left,
    y: source.clientY - rect.top
  };
}

function downloadWallpaper() {
  var link = document.createElement("a");
  link.download = "bubble-wallpaper.png";
  link.href = canvas.toDataURL("image/png");
  link.click();
}

canvas.addEventListener("click", function (event) {
  var point = canvasPoint(event);
  popBubbleAt(point.x, point.y);
});

canvas.addEventListener("touchstart", function (event) {
  var point = canvasPoint(event);
  popBubbleAt(point.x, point.y);
  event.preventDefault();
}, { passive: false });

paletteSelect.addEventListener("change", rebuildBubbles);
densityInput.addEventListener("input", rebuildBubbles);
sizeInput.addEventListener("input", rebuildBubbles);
shuffleButton.addEventListener("click", rebuildBubbles);

soundButton.addEventListener("click", function () {
  soundEnabled = !soundEnabled;
  soundButton.textContent = soundEnabled ? "Sound On" : "Sound Off";
  soundButton.setAttribute("aria-pressed", String(soundEnabled));
});

textButton.addEventListener("click", function () {
  textVisible = !textVisible;
  wallpaperTitle.classList.toggle("is-hidden", !textVisible);
  textButton.textContent = textVisible ? "Text On" : "Text Off";
  textButton.setAttribute("aria-pressed", String(textVisible));
});

downloadButton.addEventListener("click", downloadWallpaper);

fullscreenButton.addEventListener("click", function () {
  var stage = document.querySelector(".wallpaper-stage");

  if (document.fullscreenElement) {
    document.exitFullscreen();
  } else if (stage.requestFullscreen) {
    stage.requestFullscreen();
  }
});

window.addEventListener("resize", function () {
  resizeCanvas();
  rebuildBubbles();
});

resizeCanvas();
rebuildBubbles();
requestAnimationFrame(frame);
