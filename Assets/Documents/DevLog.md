# PROJECT Y - Development Log (บันทึกการพัฒนา)

> **ข้อมูลโปรเจกต์:**
> - **ชื่อเกม**: PROJECT Y (GENZ ตีเก๊ ALL THE WAY)
> - **แนวเกม**: 2D Top-Down Roguelike / Survival Action
> - **ธีม**: Dark Fantasy
> - **แพลตฟอร์ม**: Mobile แนวตั้ง (Portrait)
> - **การควบคุมหลัก**: On-Screen Virtual Joystick

---

## 📅 บันทึกความคืบหน้ารายวัน (Daily Logs)

### [2026-10-01] - เริ่มต้นและวางโครงสร้างโปรเจกต์
- **วิเคราะห์เอกสารเกม (GDD Analysis)**:
  - ศึกษาข้อกำหนดจากเอกสารนำเสนอเกมแนว Roguelike บนมือถือ (มุมมอง 2D Top-Down แนวตั้ง, Permadeath, RNG สุ่มสกิล/ด่าน, Core Loop)
- **ตรวจสอบโปรเจกต์และโค้ดเริ่มต้น**:
  - สำรวจโครงสร้างโฟลเดอร์ Unity ในโฟลเดอร์ `Assets/`
  - ตรวจสอบโค้ดทดสอบจอยสติ๊กเบื้องต้นใน `PlayerController.cs` (พบว่าเป็นตรรกะ 3D เตรียมปรับเปลี่ยนเป็น 2D Physics)
- **ระบบควบคุม (Controls)**:
  - จัดทำ On-Screen Virtual Joystick บน UI Canvas ใน Hierarchy สำหรับหน้าจอ Mobile เรียบร้อยแล้ว
  - ปรับปรุงและ Refactor `PlayerController.cs` เป็นระบบ 2D Top-Down แบบกระชับ คลีน โค้ดสั้น อ่านง่าย ไม่ซับซ้อน (Unity 6 `linearVelocity`, Null-coalescing, One-line Sprite Flip)
- **ระบบฉากและสิ่งกีดขวาง (Environment & Obstacles Mockup)**:
  - สร้าง Prefabs สิ่งกีดขวางใน `Assets/Prefabs/`: `Obstacle_Pole.prefab` (เสาหิน) และ `Obstacle_Rock.prefab` (ก้อนหิน) พร้อมใส่ Collider 2D ครบถ้วน
  - สร้าง Mockup ฉาก Arena มีพื้น (Ground) และกำแพงขอบฉาก (Wall) พร้อมติดตั้ง Collider 2D ล้อมรอบป้องกันการเดินทะลุ
  - พัฒนาระบบ `ArenaSpawner.cs` สุ่มกระจายเสาหินและก้อนหินในลาน Arena แบบอัตโนมัติ
  - ปรับปรุงการตรวจสอบระยะสุ่มสิ่งกีดขวาง: คำนวณจากขอบนอกของวัตถุ (Edge-to-Edge Clearance) และเพิ่มระยะเผื่อ `edgePadding` ป้องกันเสาหินและก้อนหินทับกัน พร้อม `Physics2D.SyncTransforms()`
  - Refactor โค้ด `ArenaSpawner.cs` ให้กระชับ สั้นลงกว่า 50% (จาก 165 บรรทัดเหลือ ~80 บรรทัด) โดยใช้ C# Pattern Matching Switch, Value Tuples และ Compact Checks โดยฟังก์ชันคงเดิม 100%
- **การปรับปรุงและเพิ่มประสิทธิภาพโค้ด (Code Refactoring & Optimization)**:
  - เคลียร์และจัดระเบียบโค้ดทั้ง 5 ไฟล์ใน `Assets/Scripts/` (`PlayerController.cs`, `CameraFollow2D.cs`, `EnemyController.cs`, `EnemySpawner.cs`, `ArenaSpawner.cs`) ให้กระชับ สั้น คลีนตา ไร้ Spaghetti Code
  - ประยุกต์ใช้เทคนิค **Data Structures & Algorithms (DS&A)**:
    - ใช้งาน **Squared Magnitude Comparison (`sqrMagnitude`)** แทน `Vector2.Distance` ในการตรวจสอบระยะสุ่ม เพื่อตัดการคำนวณรากที่สอง (Square Root) ใน Loop ออก เพิ่มประสิทธิภาพ ($O(1)$)
    - ใช้งาน **Value Tuples** และ **Pattern Matching Switch Expression** ลด Overhead การจัดสรรหน่วยความจำ
    - รวมศูนย์ฟังก์ชันการหันหน้าตัวละคร `HandleFacing()` ให้มีมาตรฐานเดียวกันทั้งผู้เล่นและศัตรู
- **สร้างเอกสารอธิบายโค้ด (Documentation)**:
  - สร้างไฟล์ใหม่ `Assets/Documents/ScriptReference.md` อธิบายหน้าที่ของแต่ละสคริปต์และแจกแจงการทำงานของแต่ละฟังก์ชันอย่างละเอียดทุกเมธอด
- **สร้างระบบบันทึกงาน**:
  - สร้างโฟลเดอร์ `Assets/Documents/` และไฟล์ `DevLog.md` สำหรับจดบันทึกการพัฒนาอย่างต่อเนื่อง

---

## 📋 แผนงานถัดไป (Upcoming Tasks / Roadmap)
- [x] ติดตั้ง/จัดวาง On-Screen Joystick สำหรับจอ Mobile แนวตั้งใน Scene (เสร็จสิ้น)
- [x] ปรับปรุง `PlayerController.cs` ให้เป็นระบบ 2D Top-Down (ใช้ Rigidbody2D, Vector2, Sprite Flip) (เสร็จสิ้น)
- [x] ออกแบบระบบ Camera Follow 2D (กล้องติดตามผู้เล่นแบบ SmoothDamp) (เสร็จสิ้น)
- [x] ออกแบบ Mockup ฉาก Arena (Ground, Wall) และ Prefabs สิ่งกีดขวาง (Obstacle_Pole, Obstacle_Rock) (เสร็จสิ้น)
- [x] พัฒนาระบบ `ArenaSpawner.cs` สุ่มกระจาย Obstacle Prefabs ลงในลาน Arena แบบ RNG (เสร็จสิ้น)
- [x] ออกแบบและพัฒนาระบบการเคลื่อนที่ของศัตรู `EnemyController.cs` เดินไล่ตาม Player (เสร็จสิ้น)
- [x] ออกแบบและพัฒนาระบบ `EnemySpawner.cs` สุ่มเกิดมอนสเตอร์เป็น Wave อัตโนมัติ (เสร็จสิ้น)
- [ ] ออกแบบระบบการโจมตีอัตโนมัติ (Auto-Aim / Nearest Enemy Attack)
- [ ] ออกแบบระบบ Health System (HP, Damage, Die/Respawn)
- [ ] ระบบสุ่มสกิล / เลเวลอัป (Roguelike Skill Selection RNG)


