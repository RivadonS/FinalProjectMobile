# 📖 Project Y - Scripts & Functions Reference (คู่มือเจาะลึกการทำงานของโค้ด)

เอกสารนี้อธิบายการทำงานของทุกฟังก์ชันในแต่ละสคริปต์อย่างละเอียด พร้อมภาพจำลองเชิงแนวคิด (Concept & Visual Breakdown) และตรรกะเบื้องหลัง เพื่อให้เข้าใจภาพรวมและนำไปใช้อธิบายหรือต่อยอดได้ง่ายที่สุดครับ

---

## 1. 🎮 `PlayerController.cs` (ระบบควบคุมตัวละคร 2D)

### 📌 หน้าที่หลัก:
แปลงสัญญาณทัชจาก Virtual Joystick บนหน้าจอมือถือ ให้กลายเป็นการเคลื่อนที่ด้วยฟิสิกส์ 2D Top-Down และจัดการการหันหน้าของตัวละครพร้อมกับอุปกรณ์ตกแต่ง (เช่น ลูกตา Eyes)

```
[Virtual Joystick Input]
       ↓ (Vector2 moveInput)
  FixedUpdate() ──→ rb.linearVelocity = moveInput * moveSpeed  (เคลื่อนที่ 2D)
  Update()      ──→ HandleFacing(moveInput.x)                  (กลับด้านตัวละคร + ตา)
```

---

### 🔍 เจาะลึกรายฟังก์ชัน:

#### `Awake()`
* **สิ่งที่ทำ**:
  1. ดึงคอมโพเนนต์ `Rigidbody2D` มาเก็บไว้
  2. ตั้งค่า `rb.gravityScale = 0f` $\rightarrow$ เพื่อไม่ให้ตัวละครร่วงตกจอในมุมมอง Top-Down
  3. ตั้งค่า `rb.freezeRotation = true` $\rightarrow$ ล็อกแกน Z ป้องกันตัวละครหมุนเคว้งตีลังกาเวลาเดินชนสิ่งกีดขวาง
  4. ตั้งค่า `rb.interpolation = RigidbodyInterpolation2D.Interpolate` $\rightarrow$ **จุดสำคัญที่สุด!** เพื่อให้ Unity คำนวณภาพแทรกระหว่างเฟรมฟิสิกส์ (50Hz) กับเฟรมหน้าจอ (60/120Hz) ทำให้การเดินนุ่มนวล กล้องไม่สั่น
  5. สร้าง Instance ของ `PlayerControls` และสมัครรับ Event จาก Joystick:
     * `performed`: เมื่อนิ้วแตะและลากจอยสติ๊ก $\rightarrow$ อัปเดตค่า `moveInput`
     * `canceled`: เมื่อยกนิ้วปล่อยจอยสติ๊ก $\rightarrow$ รีเซ็ต `moveInput = Vector2.zero`

#### `OnEnable()` / `OnDisable()`
* **สิ่งที่ทำ**: สั่งเปิด (`Enable`) และปิด (`Disable`) การรับ Input ของระบบ New Input System ตามรอบชีวิตของ GameObject ป้องกันไม่ให้รับสัญญาณค้างตอนซ่อนตัวละคร

#### `Update()`
* **สิ่งที่ทำ**: เรียกฟังก์ชัน `HandleFacing(moveInput.x)` ในทุกเฟรมเรนเดอร์ เพื่อเช็คว่าผู้เล่นกำลังขยับนิ้วไปทางซ้ายหรือขวา และสั่งกลับด้านตัวละครทันที

#### `FixedUpdate()`
* **สิ่งที่ทำ**: สั่งให้ตัวละครเคลื่อนที่ตามรอบเวลาฟิสิกส์ที่คงที่:
  ```csharp
  rb.linearVelocity = moveInput * moveSpeed;
  ```
  *(ใช้ `linearVelocity` ซึ่งเป็น API มาตรฐานใหม่ของ Unity 6 ช่วยให้การชนกับกำแพงและสิ่งกีดขวางไม่ทะลุและไม่มีแรงเสียดทานหน่วง)*

#### `HandleFacing(float dirX)`
* **ภาพจำลองการทำงาน**:
  ```
  เดินไปทางขวา (dirX > 0)    ──→ Scale.x = +1  (หน้าเดิม)
  เดินไปทางซ้าย (dirX < 0)    ──→ Scale.x = -1  (กลับด้านแบบกระจกเงา)
  ```
* **ทำไมต้องแก้ `transform.localScale.x` แทน `spriteRenderer.flipX`?**:
  * ถ้าใช้ `flipX` จะกลับด้านเฉพาะภาพตัวละคร แต่ **ลูกตา (Eyes)** ที่เป็น GameObject ลูกจะยังค้างอยู่ที่พิกัดเดิมทางขวา
  * แต่การคูณ `-1` เข้าไปที่ `localScale.x` ของตัวแม่ จะทำให้ตัวแม่และ **วัตถุที่เป็นลูกทั้งหมด (ตา, เงา, อาวุธ)** พลิกด้านตามไปด้วยแบบกระจกเงาอย่างสมบูรณ์

---

## 2. 📷 `CameraFollow2D.cs` (ระบบกล้องเคลื่อนที่ตามผู้เล่น)

### 📌 หน้าที่หลัก:
นำตำแหน่งของผู้เล่นมาคำนวณและเลื่อนกล้องตามอย่างนุ่มนวลด้วยอัลกอริทึม **Smooth Damping** โดยไม่มีการกระตุก

```
[ตำแหน่ง Player (X, Y)] + [Offset (0, 0, -10)]
                     ↓
        Vector3.SmoothDamp (ค่อยๆ ลื่นไหลตาม)
                     ↓
             [ตำแหน่ง Camera]
```

---

### 🔍 เจาะลึกรายฟังก์ชัน:

#### `Awake()`
* **สิ่งที่ทำ**: ค้นหาตัวละครในฉากอัตโนมัติด้วยคำสั่ง:
  ```csharp
  if (!target) target = FindAnyObjectByType<PlayerController>()?.transform;
  ```
* **ประโยชน์**: ผู้พัฒนาไม่ต้องเสียเวลาลากตัวละครใส่ช่อง Target ใน Inspector ทุกครั้งที่เปิด Scene ใหม่

#### `LateUpdate()`
* **ทำไมต้องใช้ `LateUpdate` แทน `Update`?**:
  * `Update` / `FixedUpdate` จะเคลื่อนที่ตัวละครก่อน
  * `LateUpdate` จะทำงานเป็นลำดับสุดท้ายของเฟรม เมื่อตัวละครขยับเสร็จแล้ว กล้องจึงค่อยขยับตาม ทำให้ระยะห่างระหว่างกล้องกับผู้เล่นไม่เกิดการสั่นสะบัด (Jitter Free)
* **การล็อกแกน Z**:
  ```csharp
  Vector3 targetPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
  ```
  * ล็อกแกน Z ไว้ที่ `-10f` เสมอ เพื่อให้กล้องถอยห่างจากระนาบเกม 2D ออกมา 10 หน่วย ทำให้มองเห็น Sprite ทั้งหมดอย่างถูกต้อง

---

## 3. 👾 `EnemyController.cs` (ระบบ AI มอนสเตอร์เดินตาม)

### 📌 หน้าที่หลัก:
ค้นหาผู้เล่นในฉาก และสร้างแรงขับเคลื่อนให้มอนสเตอร์วิ่งตรงเข้าหาผู้เล่นตลอดเวลา พร้อมหันหน้าตาม

```
  [มอนสเตอร์]  ──────── Vector2 direction ────────>  [ผู้เล่น (Target)]
        ↓
  rb.linearVelocity = direction * moveSpeed
  HandleFacing(direction.x)
```

---

### 🔍 เจาะลึกรายฟังก์ชัน:

#### `Awake()`
* **สิ่งที่ทำ**: ตั้งค่า `Rigidbody2D` เช่นเดียวกับผู้เล่น (ปิดแรงโน้มถ่วง, ล็อกการหมุน, เปิด `Interpolate`) เพื่อให้มอนสเตอร์เคลื่อนที่ด้วยฟิสิกส์ 2D ชนกับสิ่งกีดขวางได้โดยไม่ติดบั๊ก

#### `Start()`
* **สิ่งที่ทำ**: เรียกคำสั่ง `LocatePlayer()` ทันทีที่เกิดมา เพื่อล็อคเป้าหมายตั้งแต่เฟรมแรก

#### `FixedUpdate()`
* **สิ่งที่ทำ**:
  1. ถ้าเป้าหมายหลุด ให้เรียก `LocatePlayer()` ซ้ำ
  2. คำนวณเวกเตอร์บอกทิศทาง:
     $$\vec{Direction} = \frac{\vec{PlayerPos} - \vec{EnemyPos}}{|\vec{PlayerPos} - \vec{EnemyPos}|}$$
     *(ใช้ `.normalized` เพื่อให้ได้เวกเตอร์ทิศทางความยาว 1 หน่วย)*
  3. สั่งเคลื่อนที่: `rb.linearVelocity = direction * moveSpeed;`
  4. เรียก `HandleFacing(direction.x)` เพื่อหันหน้าและลูกตาไปทางที่ผู้เล่นยืนอยู่

#### `LocatePlayer()`
* **สิ่งที่ทำ**: ค้นหา GameObject ที่มี Tag เป็น `"Player"` ก่อน หากไม่มี จะค้นหาผ่านคอมโพเนนต์ `PlayerController` เป็นแผนสำรอง

#### `HandleFacing(float dirX)`
* **สิ่งที่ทำ**: กลับแกน `localScale.x` สลับซ้าย-ขวา เช่นเดียวกับตัวผู้เล่น ทำให้ลูกตาของมอนสเตอร์หันมองจ้องผู้เล่นตลอดเวลา

---

## 4. 🌀 `EnemySpawner.cs` (ระบบสร้างคลื่นศัตรูอัตโนมัติ)

### 📌 หน้าที่หลัก:
สร้างมอนสเตอร์ขึ้นมารอบตัวผู้เล่นเป็นระยะๆ โดยสุ่มเกิดอยู่นอกระยะสายตาของกล้อง ทำให้เหมือนมอนสเตอร์กำลังเดินเข้ามาล้อมผู้เล่น

```
                        [ขอบจอกล้อง]
                             ┌───────────────┐
         (👾 มอนสเตอร์เกิด)   │               │
                 ★ ──────────│   😎 ผู้เล่น   │
          (ระยะ 8 - 12 หน่วย)│               │
                             └───────────────┘
```

---

### 🔍 เจาะลึกรายฟังก์ชัน:

#### `Start()`
* **สิ่งที่ทำ**:
  1. ค้นหาพิกัดของผู้เล่น
  2. เสกมอนสเตอร์ชุดแรกทันทีตามจำนวน `initialCount` (เช่น 5 ตัว)
  3. เรียกใช้ฟังก์ชันจับเวลา `InvokeRepeating(nameof(SpawnWave), spawnInterval, spawnInterval)` เพื่อให้เรียกฟังก์ชัน `SpawnWave` ซ้ำๆ ทุกๆ `spawnInterval` วินาที

#### `SpawnWave()`
* **สิ่งที่ทำ**:
  * ตรวจสอบเงื่อนไขความปลอดภัย: มอนสเตอร์ในฉากต้องไม่เกินขีดจำกัด (`transform.childCount < maxEnemies`)
  * ทำการวนลูปเรียก `SpawnSingle()` ตามจำนวน `spawnPerWave` (เช่น เสกเพิ่มระลอกละ 2 ตัว)

#### `SpawnSingle()`
* **ตรรกะการสุ่มพิกัด**:
  ```csharp
  Vector2 randomDir = Random.insideUnitCircle.normalized; // สุ่มทิศทาง 360 องศารอบตัว
  float distance = Random.Range(minDistance, maxDistance); // สุ่มระยะห่าง 8 ถึง 12 เมตร (นอกจอ)
  Vector2 spawnPos = (Vector2)playerTransform.position + (randomDir * distance);
  ```
  * สุ่มจุดเกิดเป็นวงแหวนรอบตัวผู้เล่น แล้วสั่ง `Instantiate` โดยส่ง `transform` ของ Spawner เป็น Parent เพื่อให้มอนสเตอร์ทั้งหมดรวมอยู่ในโฟลเดอร์เดียวกันใน Hierarchy

#### `OnDrawGizmosSelected()`
* **สิ่งที่ทำ**: วาดวงกลม Gizmos สีแดง (`minDistance`) และสีส้ม (`maxDistance`) ในหน้าต่าง Scene View ช่วยให้มองเห็นขอบเขตระยะเกิดของศัตรูได้ด้วยสายตา

---

## 5. 🏛️ `ArenaSpawner.cs` (ระบบสุ่มสิ่งกีดขวางแบบ Procedural Generation)

### 📌 หน้าที่หลัก:
สุ่มวางเสาหินและก้อนหินลงบนลานประลอง โดยรับประกันว่า:
1. **ไม่เกิดทับผู้เล่น** (มี Safe Zone)
2. **ไม่เกิดซ้อนทับกันเอง** โดยคำนวณจาก **"ขอบนอกชนขอบนอก (Edge-to-Edge)"**
3. **ไม่เกิดทับกำแพงฉาก**

---

### 💡 อัลกอริทึม & Data Structures ที่นำมาประยุกต์ใช้ (DS&A Breakdown):

#### 1. การคำนวณระยะชนแบบ Edge-to-Edge (ขอบชนขอบ)
```
          [หินชิ้นเดิม]                [เสาหินชิ้นใหม่]
          ( รัศมี R1 )                  ( รัศมี R2 )
          ╭─────────╮   padding        ╭─────────╮
          │    ●    │ ◄─────────►      │    ●    │
          ╰─────────╯                  ╰─────────╯
               └───────── ระยะห่างรวม ─────────┘
            Distance >= R1 + R2 + edgePadding
```
* หากคำนวณจากจุดกึ่งกลาง (Center) เสาหินชิ้นใหญ่อาจจะเกยทับก้อนหินได้
* โค้ดนี้จึงคำนวณขนาดจริง (`Radius`) ของ Collider แต่ละชิ้นก่อน แล้วบังคับให้ระยะห่างระหว่างจุดกึ่งกลางต้องมากกว่า **$(R_1 + R_2 + \text{Padding})$** เสมอ

#### 2. เทคนิค Squared Distance (`sqrMagnitude`) ปรับความเร็ว $O(1)$
* ปกติการหาระยะห่าง $Distance = \sqrt{\Delta x^2 + \Delta y^2}$ จะมีฟังก์ชัน **Square Root ($\sqrt{x}$)** ซึ่งกินพลังประมวลผลสูงมากเมื่อทำงานในลูป
* **วิธีแก้ตามหลักคณิตศาสตร์/อัลกอริทึม**:
  $$\text{ถ้า } A < B \iff A^2 < B^2$$
  ดังนั้น เราจึงเปรียบเทียบระยะยกกำลังสองแทน:
  ```csharp
  // เร็วกว่า Vector2.Distance อย่างมหาศาล เพราะไม่ต้องถอดสแควรูท
  if ((newPos - pos).sqrMagnitude < (requiredClearance * requiredClearance))
      return true; // ชนกันแน่นอน
  ```

---

### 🔍 เจาะลึกรายฟังก์ชัน:

#### `Start()`
* **สิ่งที่ทำ**: ค้นหาผู้เล่น และสั่งเรียก `SpawnObstacles()` ทันทีตอนเริ่มเกม

#### `SpawnObstacles()`
* **ภาพรวมการทำงานในลูป**:
  1. สุ่มจำนวนเป้าหมาย (`targetCount`) เช่น สุ่มระหว่าง 8 ถึง 15 ชิ้น
  2. เริ่มลูปสุ่มพิกัด `pos` ภายในพื้นที่สี่เหลี่ยม Arena
  3. **ด่านตรวจที่ 1**: ตรวจสอบว่าพิกัดนั้นอยู่ใกล้ผู้เล่นเกินไปหรือไม่?
     * `(pos - playerPos).sqrMagnitude < (playerSafeDist * playerSafeDist)` $\rightarrow$ ถ้าใช่ ให้สุ่มใหม่
  4. **ด่านตรวจที่ 2**: เรียก `IsOverlappingWithEdges(pos, radius)` ตรวจสอบว่าขอบนอกไปชนกับสิ่งกีดขวางที่เคยวางไว้ก่อนหน้าหรือไม่?
  5. **ด่านตรวจที่ 3**: เรียก `Physics2D.OverlapCircle` ตรวจสอบว่าไปทับกำแพงฉากหรือไม่?
  6. **ผ่านทุกด่าน**: สั่ง `Instantiate` วางลงในฉาก $\rightarrow$ เรียก `Physics2D.SyncTransforms()` เพื่ออัปเดตระบบฟิสิกส์ทันที $\rightarrow$ บันทึกพิกัดและขนาดลงใน `spawnedList`

#### `IsOverlappingWithEdges(Vector2 newPos, float newRadius)`
* **สิ่งที่ทำ**: วนลูปตรวจสอบพิกัดใหม่เทียบกับทุกชิ้นใน `spawnedList` โดยใช้สูตร Squared Distance ข้างต้น หากพบว่าชิดเกินไปจะคืนค่า `true` ทันที (Early Exit)

#### `GetObjectRadius(GameObject obj)`
* **สิ่งที่ทำ**: คำนวณหา "รัศมีขอบนอกสุด" ของ Prefab โดยใช้ **C# Pattern Matching Switch**:
  * ถ้าเป็น `CircleCollider2D` $\rightarrow$ รัศมี = `radius * scale`
  * ถ้าเป็น `BoxCollider2D` หรือ `CapsuleCollider2D` $\rightarrow$ นำครึ่งหนึ่งของขนาด (`size * 0.5f`) มาหาความยาวเวกเตอร์ทะแยงมุม
  * คืนค่าขนาดที่แม่นยำที่สุดกลับไปใช้คำนวณระยะเว้นขอบ

#### `OnDrawGizmosSelected()`
* **สิ่งที่ทำ**: วาดกล่อง Wireframe สี่เหลี่ยมสีฟ้า (แสดงขอบเขตของ Arena) และวงกลมสีเหลือง (แสดงระยะปลอดภัยของผู้เล่น) ใน Scene View ช่วยให้เห็นขอบเขตด่านชัดเจนใน Unity
