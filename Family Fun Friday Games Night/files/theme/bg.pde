color c;
float x, y, s;
void setup() {
  size(window.innerWidth, window.innerHeight);
  background(0, 0);
  smooth();
  noStroke();
  ellipseMode(RADIUS);
}
void draw() {
  c = color(random(255), random(255), random(255), random(255));
  x = random(width);
  y = random(height);
  s = random(100);
  fill(c);
  ellipse(x, y, s, s);
}
