shader_type canvas_item;

const uint POINT_COUNT = uint(20);
const float BORDER_WIDTH = 3.5f;
const vec4 BORDER_COLOR = vec4(0.5, 0.8, 1.0, 0.5);
const vec4 BODY_COLOR = vec4(0.4, 0.5, 1.0, 0.5);

//const vec2 points[] = {vec2(0,0), vec2(100, 0), vec2(200, 200), vec2(300, 0), vec2(400, 0)};
uniform vec2 points[POINT_COUNT];
varying vec2 world_vertex;

vec2 rot_point_around(vec2 bp, vec2 rp, float rangle) {
	float x1 = rp.x - bp.x;
	float y1 = rp.y - bp.y;
	float x2 = x1 * cos(rangle) - y1 * sin(rangle);
	float y2 = x1 * sin(rangle) + y1 * cos(rangle);
	return vec2(
		x2 + bp.x,
		y2 + bp.y
	);
}

float rangle_between_points(vec2 bp, vec2 rp) {
	return atan((bp.y - rp.y) / (rp.x - bp.x));
}

bool is_point_on_line(vec2 p, vec2 lp1, vec2 lp2, float lw, bool smoothout) {
	// todo: smoothout
	float rangle = rangle_between_points(lp1, lp2);
	lp2 = rot_point_around(lp1, lp2, rangle);
	p = rot_point_around(lp1, p, rangle);
	float minx = min(lp1.x, lp2.x);
	float maxx = max(lp1.x, lp2.x);
	return distance(p, lp1) <= lw
		|| distance(p, lp2) <= lw
		|| p.x >= minx && p.x <= maxx && p.y >= (lp1.y - lw) && p.y <= (lp2.y + lw);
}

void vertex() {
	world_vertex = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
	COLOR = vec4(texture(TEXTURE, UV).rgb, 0);
	bool is_on_any_line = false;
	for(int i = 0; i < points.length(); i++) {
		if(i > 0) {
			if(is_point_on_line(world_vertex, points[i-1], points[i], BORDER_WIDTH, true)) {
				is_on_any_line = true;
				COLOR = BORDER_COLOR;
			}
		}
	}
	for(int i = 0; i < points.length(); i++) {
		if(i > 0) {
			if(!is_on_any_line) {
				float rangle = rangle_between_points(points[i-1], points[i]);
				vec2 point = rot_point_around(points[i-1], world_vertex, rangle);
				if(world_vertex.x >= points[i-1].x - BORDER_WIDTH && world_vertex.x <= points[i].x + BORDER_WIDTH && point.y >= points[i-1].y) {
					COLOR = BODY_COLOR;
				}
			}
		}
	}
}