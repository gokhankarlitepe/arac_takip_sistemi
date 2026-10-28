import math
from PIL import Image, ImageDraw, ImageFilter

def create_app_icon():
    # 512x512 master resolution for super-crisp downsampling
    size = 512
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 1. Rounded Squircle Background with Rich Modern Gradient
    # Deep royal blue to electric cyan/indigo
    margin = 24
    radius = 110
    
    # Create mask for rounded rect
    mask = Image.new("L", (size, size), 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.rounded_rectangle([margin, margin, size - margin, size - margin], radius=radius, fill=255)

    # Generate vertical gradient
    gradient = Image.new("RGBA", (size, size))
    g_draw = ImageDraw.Draw(gradient)
    for y in range(size):
        ratio = y / size
        # #1e1b4b (dark indigo) to #1d4ed8 to #0284c7 (vibrant cyan-blue)
        r = int(18 + (30 - 18) * ratio)
        g = int(35 + (120 - 35) * ratio)
        b = int(110 + (225 - 110) * ratio)
        g_draw.line([(0, y), (size, y)], fill=(r, g, b, 255))

    # Apply rounded rect mask to gradient
    img.paste(gradient, (0, 0), mask)

    # 2. Subtle Gloss / Inner Glow Border
    border_draw = ImageDraw.Draw(img)
    border_draw.rounded_rectangle([margin, margin, size - margin, size - margin], radius=radius, outline=(255, 255, 255, 70), width=6)
    border_draw.rounded_rectangle([margin+4, margin+4, size - margin - 4, size - margin - 4], radius=radius-4, outline=(56, 189, 248, 60), width=3)

    # 3. Modern Car Silhouette (Vector Paths)
    # Scaled and centered at (256, 280)
    car_overlay = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cd = ImageDraw.Draw(car_overlay)

    # Car Body Coordinates (Sleek aerodynamic sedan / SUV)
    # Hood, windshield, roof, rear, bumpers
    # Car shadow
    cd.ellipse([100, 395, 412, 435], fill=(0, 0, 0, 90))

    # Main Car Body (White / Silver with subtle highlight)
    body_poly = [
        (95, 360),    # front bumper bottom
        (85, 335),    # front nose
        (130, 305),   # hood front
        (190, 285),   # hood base
        (230, 215),   # A-pillar bottom
        (280, 195),   # roof start
        (355, 195),   # roof end
        (405, 260),   # rear window
        (425, 290),   # trunk
        (432, 335),   # rear bumper
        (420, 360),   # rear bumper bottom
        (380, 360),   # rear wheel arch right
        (370, 325),   # rear wheel arch top
        (325, 325),   # rear wheel arch left
        (315, 360),   # bottom chassis mid
        (200, 360),   # front chassis mid
        (190, 325),   # front wheel arch right
        (145, 325),   # front wheel arch top
        (135, 360),   # front wheel arch left
    ]
    cd.polygon(body_poly, fill=(248, 250, 252, 255))

    # Car Cabin / Windows (Dark Tinted Glass #0f172a with cyan reflection)
    window_front = [
        (238, 225),
        (280, 206),
        (280, 280),
        (195, 285)
    ]
    cd.polygon(window_front, fill=(15, 23, 42, 240))
    
    window_rear = [
        (292, 206),
        (350, 206),
        (395, 265),
        (395, 280),
        (292, 280)
    ]
    cd.polygon(window_rear, fill=(15, 23, 42, 240))

    # Window Reflection Glare (Cyan Accent)
    cd.line([(246, 230), (275, 275)], fill=(56, 189, 248, 160), width=5)
    cd.line([(305, 215), (345, 275)], fill=(56, 189, 248, 130), width=5)

    # Wheels (Alloy Wheels & Tires)
    # Front Wheel (center ~162, 345, r=32)
    def draw_wheel(cx, cy, r):
        # Tire
        cd.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(30, 41, 59, 255), outline=(15, 23, 42, 255), width=4)
        # Rim
        rim_r = r - 10
        cd.ellipse([cx - rim_r, cy - rim_r, cx + rim_r, cy + rim_r], fill=(226, 232, 240, 255))
        # Rim Center & Spokes
        cd.ellipse([cx - 5, cy - 5, cx + 5, cy + 5], fill=(30, 41, 59, 255))
        for angle in range(0, 360, 60):
            rad = math.radians(angle)
            x2 = cx + int(math.cos(rad) * rim_r)
            y2 = cy + int(math.sin(rad) * rim_r)
            cd.line([(cx, cy), (x2, y2)], fill=(71, 85, 105, 255), width=3)

    draw_wheel(162, 345, 34)
    draw_wheel(348, 345, 34)

    # Headlight Glow (Emerald / Amber check pulse)
    cd.polygon([(90, 320), (120, 310), (115, 330), (88, 335)], fill=(56, 189, 248, 255)) # Xenon Blue
    # Taillight
    cd.polygon([(422, 315), (430, 318), (428, 332), (418, 330)], fill=(239, 68, 68, 255)) # Ruby Red

    img = Image.alpha_composite(img, car_overlay)

    # 4. Top-Right Badge: GPS Location Pin / Security Check Shield
    badge = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    bd = ImageDraw.Draw(badge)
    
    bx, by = 370, 115
    br = 50
    # Badge Shadow
    bd.ellipse([bx - br + 4, by - br + 6, bx + br + 4, by + br + 6], fill=(0, 0, 0, 80))
    # Emerald Gradient Circle
    bd.ellipse([bx - br, by - br, bx + br, by + br], fill=(16, 185, 129, 255), outline=(255, 255, 255, 240), width=5)
    
    # Checkmark inside badge (White)
    check_coords = [
        (bx - 22, by - 2),
        (bx - 7, by + 16),
        (bx + 22, by - 14)
    ]
    bd.line(check_coords, fill=(255, 255, 255, 255), width=9, joint="curve")

    img = Image.alpha_composite(img, badge)

    # 5. Export to Multi-Resolution .ICO format
    icon_sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
    icon_images = [img.resize(s, Image.Resampling.LANCZOS) for s in icon_sizes]

    img.save("app_icon.png")
    icon_images[0].save(
        "app_icon.ico",
        format="ICO",
        sizes=icon_sizes,
        append_images=icon_images[1:]
    )
    print("App icon generated successfully: app_icon.ico and app_icon.png")

if __name__ == "__main__":
    create_app_icon()
