"""Drift track layout: centre line, widths, edge styles, banking and rails."""
import math

# Arena (metres). x east, z south. Terrain is a 1 m height grid on whole metres.
X0, X1, Z0, Z1 = -170, 170, -130, 130
NX, NZ = X1 - X0 + 1, Z1 - Z0 + 1

# Corner waypoints:
# (x, z, radius, width after this corner, left style, right style, bank degrees, bank lift)
# Styles: g = grass run-off, d = ditch, r = rail with exit gaps, as seen by the driver.
# Bank lift: share of the half width the centre line is raised, so the high side stands
# taller than the low side digs in. 0 keeps the centre line at ground level.
CORNERS = [
    ( 180.2,  95.0, 55.0, 16, 'g', 'g', 10, 0.0),   # 0  S-curve: left, banked
    ( 126.5, -20.1, 55.0, 16, 'g', 'g', 10, 0.0),   # 1  S-curve: right, banked the other way
    ( 145.0, -59.8, 55.0, 18, 'g', 'd',  0, 0.0),   # 2  eases onto the long straight
    ( 145.0,-115.0,  9.0, 14, 'g', 'r',  0, 0.0),   # 3  tight U-hairpin: two r=9 corners
    ( 121.0,-115.0,  9.0, 12, 'g', 'g',  0, 0.0),   # 4
    ( 121.0, -52.0, 18.0, 12, 'd', 'd',  0, 0.0),   # 5  back straight; jumps over leg 8
    ( -40.0, -52.0,  0.1, 12, 'g', 'g',  0, 0.0),   # 6  end of the landing, road stays narrow
    (-111.5, -52.0, 15.5, 17, 'g', 'g',  0, 0.0),   # 7  passage hairpin: in
    (-111.5, -14.0, 15.5, 17, 'g', 'g',  0, 0.0),   # 8  passage hairpin: out
    (  10.0,  -5.0, 12.0, 13, 'g', 'g',  0, 0.0),   # 9  north under the overpass jump
    (  10.0,-105.0, 12.0, 14, 'g', 'd',  0, 0.0),   # 10
    ( -60.0,-105.0, 30.0, 16, 'g', 'r',  0, 0.0),   # 11 carousel: long left-hander
    (-140.0, -60.0, 30.0, 16, 'g', 'r',  0, 0.0),   # 12
    (-140.0,  95.0, 70.0, 16, 'g', 'g', 14, 0.4),   # 13 big banked corner onto the start straight
]
SPUR = [(-60, 95), (-60, 58)]       # access road from the start straight to the parking lot
SPUR_WIDTH = 10
LOT = (-100, -10, 10, 58)           # parking lot rectangle x0, x1, z0, z1
PAD = (-4, 95, 16, 78)              # jump test pad in the infield, same asphalt as the lot
CROSSING = (10, -52)                # leg 5 jumps over leg 9 here
NO_DITCH = [CROSSING, (124, -56)]   # no ditches near these: the crossing, the S-curve's shoulder

# Passage before the hairpin. Rails are lists of (x, z) points.
PASSAGE_PAINT = (-96, -66, -56, -44)    # asphalt x0, x1, z0, z1 under the passage
PASSAGE_OUTER = [(-68.0, -59.5), (-80.0, -56.0), (-92.0, -56.0), (-108.5, -58.2)]
PASSAGE_INNER_START = [(-54.0, -41.0), (-64.0, -44.0)]
INNER_OFFSET = 8.0                  # hairpin rails: metres from the centre line
OUTER_OFFSET = 9.5
HAIRPIN_IN, HAIRPIN_OUT = 7, 8      # corner numbers
INNER_RAIL_END_X = -66.0

BANK_TRIM = 6.0                     # metres inside each end of a corner where the bank is held
BANK_EASE = 10                      # samples either side, smoothed twice along the road
SHOULDER = 12.0                     # metres beyond the road edge where a bank meets the ground


def rot(v, a):
    c, s = math.cos(a), math.sin(a)
    return (c * v[0] - s * v[1], s * v[0] + c * v[1])


def smooth(u):
    u = min(1.0, max(0.0, u))
    return u * u * (3 - 2 * u)


def centre_line(step=1.0):
    """Rounded loop sampled every `step` m:
    list of (x, z, dir_x, dir_z, leg index, s along leg 0..1, metres inside the arc or -1)."""
    n = len(CORNERS)
    arcs = []
    for i in range(n):
        p = CORNERS[i - 1]; c = CORNERS[i]; q = CORNERS[(i + 1) % n]
        a = (c[0] - p[0], c[1] - p[1]); b = (q[0] - c[0], q[1] - c[1])
        la, lb = math.hypot(*a), math.hypot(*b)
        a = (a[0] / la, a[1] / la); b = (b[0] / lb, b[1] / lb)
        turn = math.atan2(a[0] * b[1] - a[1] * b[0], a[0] * b[0] + a[1] * b[1])
        t = c[2] * math.tan(abs(turn) / 2)
        arcs.append(dict(start=(c[0] - a[0] * t, c[1] - a[1] * t), end=(c[0] + b[0] * t, c[1] + b[1] * t),
                         din=a, turn=turn, r=c[2], t=t, la=la, lb=lb))
    for i in range(n):  # corners must not overlap
        assert arcs[i]['t'] + arcs[(i + 1) % n]['t'] < arcs[i]['lb'] - 5, f"corners {i} and {(i + 1) % n} overlap"
    pts = []
    for i in range(n):
        A = arcs[i]; B = arcs[(i + 1) % n]
        sgn = 1 if A['turn'] > 0 else -1
        nrm = (-A['din'][1] * sgn, A['din'][0] * sgn)       # towards the arc centre
        cen = (A['start'][0] + nrm[0] * A['r'], A['start'][1] + nrm[1] * A['r'])
        A['centre'] = cen
        length = abs(A['turn']) * A['r']
        k = max(1, int(length / step))
        a0 = math.atan2(A['start'][1] - cen[1], A['start'][0] - cen[0])
        leg = []
        for j in range(k):
            ang = a0 + A['turn'] * j / k
            d = rot(A['din'], A['turn'] * j / k)
            inside = min(j, k - j) * length / k
            leg.append((cen[0] + A['r'] * math.cos(ang), cen[1] + A['r'] * math.sin(ang), d[0], d[1], inside))
        sx, sz = A['end']; ex, ez = B['start']
        L = math.hypot(ex - sx, ez - sz)
        k = max(1, int(L / step))
        d = ((ex - sx) / L, (ez - sz) / L)
        leg += [(sx + (ex - sx) * j / k, sz + (ez - sz) * j / k, d[0], d[1], -1.0) for j in range(k)]
        pts += [(x, z, dx, dz, i, j / len(leg), inside) for j, (x, z, dx, dz, inside) in enumerate(leg)]
    return pts, arcs


def half_width(leg, s):
    """Width eases from this leg's width to the next one's over the last 40% of the leg."""
    w0 = CORNERS[leg][3]; w1 = CORNERS[(leg + 1) % len(CORNERS)][3]
    u = max(0.0, (s - 0.6) / 0.4)
    u = u * u * (3 - 2 * u)
    return (w0 + (w1 - w0) * u) / 2


def bank_profile(pts, arcs):
    """Per sample: (slope towards the driver's left, centre line lift in metres)."""
    n = len(pts)
    slope = [0.0] * n
    lift = [0.0] * n
    for k, p in enumerate(pts):
        leg, inside = p[4], p[6]
        bank = CORNERS[leg][6]
        if bank and inside >= BANK_TRIM:
            tan = math.tan(math.radians(bank))
            # turn > 0 is a right-hander (x east, z south): its outside, the driver's left, goes up
            slope[k] = tan if arcs[leg]['turn'] > 0 else -tan
            lift[k] = tan * half_width(leg, p[5]) * CORNERS[leg][7]

    def ease(values):
        for _ in range(2):
            run = sum(values[-BANK_EASE:]) + sum(values[:BANK_EASE + 1])
            out = []
            for k in range(n):
                out.append(run / (2 * BANK_EASE + 1))
                run += values[(k + BANK_EASE + 1) % n] - values[k - BANK_EASE]
            values = out
        return values
    return ease(slope), ease(lift)


DITCH_SHOULDER, DITCH_IN, DITCH_BOTTOM, DITCH_OUT, DITCH_DEPTH = 0.5, 4.0, 2.5, 5.0, 1.1


def ditch_height(e):
    """Height at distance e (m) beyond the road edge."""
    e -= DITCH_SHOULDER
    if e <= 0:
        return 0.0
    if e < DITCH_IN:
        return -DITCH_DEPTH * smooth(e / DITCH_IN)
    e -= DITCH_IN
    if e < DITCH_BOTTOM:
        return -DITCH_DEPTH
    e -= DITCH_BOTTOM
    return -DITCH_DEPTH * (1 - smooth(e / DITCH_OUT))


GRASS, ROAD, DITCH, LOTC = 0, 1, 2, 3


def ditch_strength(pts, taper=18.0):
    """Per sample and side (left, right): 0..1 ditch depth factor, easing in and out over `taper` m."""
    n = len(pts)
    out = []
    for side in (4, 5):
        on = [CORNERS[p[4]][side] == 'd' for p in pts]
        dist = [0.0 if not o else 1e9 for o in on]
        for _ in range(2):  # forward and backward sweeps round the loop
            for k in range(2 * n):
                i = k % n
                if on[i]:
                    dist[i] = min(dist[i], dist[i - 1] + 1)
            for k in range(2 * n, 0, -1):
                i = k % n
                if on[i]:
                    dist[i] = min(dist[i], dist[(i + 1) % n] + 1)
        out.append([smooth(d / taper) * math.prod(smooth((math.hypot(p[0] - x, p[1] - z) - 22) / 14) for x, z in NO_DITCH)
                    for d, p in zip(dist, pts)])
    return out


def build_grid():
    pts, arcs = centre_line()
    strength = ditch_strength(pts)
    slope, lift = bank_profile(pts, arcs)
    reach = 10 + max(SHOULDER, DITCH_SHOULDER + DITCH_IN + DITCH_BOTTOM + DITCH_OUT) + 1
    BUCKET = 30   # metres of track per stretch; each nearby stretch contributes its nearest point
    near = [None] * (NX * NZ)
    for k, p in enumerate(pts):
        x, z = p[0], p[1]
        bk = k // BUCKET
        for gz in range(max(Z0, int(z - reach)), min(Z1, int(z + reach)) + 1):
            row = (gz - Z0) * NX - X0
            for gx in range(max(X0, int(x - reach)), min(X1, int(x + reach)) + 1):
                d2 = (gx - x) ** 2 + (gz - z) ** 2
                if d2 > reach * reach:
                    continue
                cell = near[row + gx]
                if cell is None:
                    near[row + gx] = cell = {}
                if d2 < cell.get(bk, (1e9,))[0]:
                    cell[bk] = (d2, k)
    nb = (len(pts) + BUCKET - 1) // BUCKET
    height = [0.0] * (NX * NZ)
    ditch = [0.0] * (NX * NZ)
    road = [False] * (NX * NZ)
    for i, cell in enumerate(near):
        if cell is None:
            continue
        gx, gz = X0 + i % NX, Z0 + i // NX
        best = 1e9
        for bk, (d2, k) in cell.items():
            if any(cell.get(nbk % nb, (1e9,))[0] < d2 for nbk in (bk - 1, bk + 1)):
                continue   # a neighbouring stretch is nearer: this is not a local nearest point
            x, z, dx, dz, leg, s, _ = pts[k]
            hw = half_width(leg, s)
            e = math.sqrt(d2) - hw
            lat = (gx - x) * dz - (gz - z) * dx    # + = driver's left (x east, z south)
            if e <= 0:
                road[i] = True
                if d2 < best:
                    height[i] = slope[k] * lat + lift[k]
                best = min(best, d2)
                continue
            f = strength[0][k] if lat > 0 else strength[1][k]
            if f > 0:
                ditch[i] = min(ditch[i], ditch_height(e) * f)
            if not road[i] and d2 < best:
                best = d2
                edge = slope[k] * (hw if lat > 0 else -hw) + lift[k]
                height[i] = edge * (1 - smooth(e / SHOULDER))
    kind = [GRASS] * (NX * NZ)
    (ax, az), (bx, bz) = SPUR
    px0, px1, pz0, pz1 = PASSAGE_PAINT
    for i in range(NX * NZ):
        gx, gz = X0 + i % NX, Z0 + i // NX
        if road[i]:
            kind[i] = ROAD
        elif px0 <= gx <= px1 and pz0 <= gz <= pz1:
            kind[i] = ROAD; height[i] = 0.0
        elif LOT[0] <= gx <= LOT[1] and LOT[2] <= gz <= LOT[3]:
            kind[i] = LOTC; height[i] = 0.0
        elif PAD[0] <= gx <= PAD[1] and PAD[2] <= gz <= PAD[3]:
            kind[i] = LOTC; height[i] = 0.0
        elif abs(gx - ax) <= SPUR_WIDTH / 2 and min(az, bz) <= gz <= max(az, bz):
            kind[i] = LOTC; height[i] = 0.0
        else:
            height[i] += ditch[i]
            if ditch[i] < -0.05:
                kind[i] = DITCH
    return pts, arcs, height, ditch, kind


def steep_steps(height, limit=0.45):
    """Neighbouring grid points more than `limit` m apart in height (a 1 m cell): likely cliffs."""
    bad = []
    for i in range(NX * NZ):
        gx, gz = i % NX, i // NX
        for j in ((i + 1) if gx + 1 < NX else None, (i + NX) if gz + 1 < NZ else None):
            if j is not None and abs(height[i] - height[j]) > limit:
                bad.append((X0 + gx, Z0 + gz, round(height[i] - height[j], 2)))
    return bad


def height_at(height, x, z):
    """Terrain height at any point, blended from the four grid points around it."""
    gx, gz = x - X0, z - Z0
    ix, iz = min(int(gx), NX - 2), min(int(gz), NZ - 2)
    fx, fz = gx - ix, gz - iz
    row = iz * NX + ix
    return ((height[row] * (1 - fx) + height[row + 1] * fx) * (1 - fz)
            + (height[row + NX] * (1 - fx) + height[row + NX + 1] * fx) * fz)


def rails(pts, offset=1.2, gap_every=40, gap=9):
    """Rail runs (lists of (x, z)) beside every rail-styled edge, broken by exit gaps."""
    runs = []
    for side in (1, -1):
        cur = []
        dist = 0.0
        for x, z, dx, dz, leg, s, _ in pts:
            style = CORNERS[leg][4] if side > 0 else CORNERS[leg][5]
            hw = half_width(leg, s)
            on = style == 'r' and (dist % (gap_every + gap)) < gap_every
            dist = dist + 1 if style == 'r' else 0.0
            if on:
                # left normal of (dx, dz) seen from above with z south is (dz, -dx)
                cur.append((x + dz * side * (hw + offset), z - dx * side * (hw + offset)))
            elif cur:
                if len(cur) > 3:
                    runs.append(cur)
                cur = []
        if len(cur) > 3:
            runs.append(cur)
    return runs


def passage_rails(pts, arcs):
    """The two rails of the passage hairpin. The outside stops short of the exit, so a wide
    slide runs out onto the grass instead of along a wall."""
    inner = list(PASSAGE_INNER_START)
    outer = list(PASSAGE_OUTER)
    centre_x = arcs[HAIRPIN_IN]['centre'][0]
    centre_z = arcs[HAIRPIN_IN]['centre'][1]
    in_turn = False
    for x, z, dx, dz, leg, s, inside in pts:
        if leg == HAIRPIN_IN or leg == HAIRPIN_OUT:
            in_turn = True
        elif in_turn:
            break
        else:
            continue
        if x <= centre_x or leg == HAIRPIN_OUT:
            if x <= INNER_RAIL_END_X:
                inner.append((x + dz * INNER_OFFSET, z - dx * INNER_OFFSET))
        beyond_flare = leg == HAIRPIN_IN and math.atan2(x - centre_x, centre_z - z) < -0.55
        on_outside_arc = leg == HAIRPIN_OUT and 0 <= inside < 12
        early_arc = leg == HAIRPIN_OUT and inside < 0 and s < 0.02
        if beyond_flare or on_outside_arc or early_arc:
            outer.append((x - dz * OUTER_OFFSET, z + dx * OUTER_OFFSET))
    return [inner, outer]
