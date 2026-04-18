# -*- coding: utf-8 -*-
"""Generate Strike/Shield/Heal/MagicStrike/Poison card assets from base Cards/*.asset"""
import os
import re
import uuid

BASE = os.path.join(
    os.path.dirname(__file__),
    "..",
    "Assets",
    "Scripts",
    "ScriptableObjects",
    "Cards",
)
BASE = os.path.normpath(BASE)

EFFECTS = [
    (
        "Strike",
        4,
        "Strike",
        "On win: deal damage to the opponent equal to this card value (shield absorbs first).",
    ),
    ("Shield", 5, "Shield", "On win: gain shield equal to this card value."),
    ("Heal", 6, "Heal", "On win: heal yourself for this card value."),
    (
        "MagicStrike",
        7,
        "MagicStrike",
        "On win: deal damage to the opponent equal to this card value, ignoring shield.",
    ),
    (
        "Poison",
        8,
        "Poison",
        "On win: apply poison stacks equal to this card value. At round start, take damage equal to current stacks, then stacks decrease by 1.",
    ),
]


def to_readable(internal_name: str) -> str:
    for suit in ("Hearts", "Diamonds", "Clubs", "Spades"):
        if internal_name.endswith(suit):
            rank = internal_name[: -len(suit)]
            return f"{rank} of {suit}"
    return internal_name


def main():
    subdirs = [e[0] for e in EFFECTS]
    for sub in subdirs:
        d = os.path.join(BASE, sub)
        for fn in os.listdir(d):
            if not fn.endswith(".asset"):
                continue
            path = os.path.join(d, fn)
            os.remove(path)
            meta = path + ".meta"
            if os.path.isfile(meta):
                os.remove(meta)

    skip = {"JockerRed.asset", "JockerBlack.asset"}
    sources = sorted(
        f
        for f in os.listdir(BASE)
        if f.endswith(".asset") and f not in skip and os.path.isfile(os.path.join(BASE, f))
    )

    for src_name in sources:
        src_path = os.path.join(BASE, src_name)
        with open(src_path, "r", encoding="utf-8") as f:
            content = f.read()

        m = re.search(r"^  m_Name: (.+)$", content, re.MULTILINE)
        orig_internal = (m.group(1).strip() if m else src_name.replace(".asset", "")).strip()

        for folder, type_id, tag, desc in EFFECTS:
            internal = f"{folder}_{orig_internal}"
            readable = to_readable(orig_internal)
            card_name = f"{tag} - {readable}"
            new_content = content
            new_content = re.sub(
                r"^  m_Name: .+$", f"  m_Name: {internal}", new_content, count=1, flags=re.MULTILINE
            )
            new_content = re.sub(
                r"^  cardName: .+$",
                f"  cardName: {card_name}",
                new_content,
                count=1,
                flags=re.MULTILINE,
            )
            desc_esc = desc.replace('"', '\\"')
            new_content = re.sub(
                r"^  description: .+$",
                f'  description: "{desc_esc}"',
                new_content,
                count=1,
                flags=re.MULTILINE,
            )
            win_block = (
                f"  onWinEffects:\n"
                f"  - type: {type_id}\n"
                f"    value: 0\n"
                f"    description: {tag}"
            )
            new_content = re.sub(
                r"^  onWinEffects: \[\]\s*$", win_block, new_content, count=1, flags=re.MULTILINE
            )
            if folder in ("MagicStrike", "Poison"):
                new_content = re.sub(
                    r"^  rarity: \d+$", "  rarity: 2", new_content, count=1, flags=re.MULTILINE
                )
                new_content = re.sub(
                    r"^  shopCost: \d+$", "  shopCost: 20", new_content, count=1, flags=re.MULTILINE
                )
            else:
                new_content = re.sub(
                    r"^  rarity: \d+$", "  rarity: 1", new_content, count=1, flags=re.MULTILINE
                )
                new_content = re.sub(
                    r"^  shopCost: \d+$", "  shopCost: 12", new_content, count=1, flags=re.MULTILINE
                )

            out_dir = os.path.join(BASE, folder)
            out_path = os.path.join(out_dir, f"{internal}.asset")
            with open(out_path, "w", encoding="utf-8", newline="\n") as fo:
                fo.write(new_content)

            meta = f"""fileFormatVersion: 2
guid: {uuid.uuid4().hex}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
            with open(out_path + ".meta", "w", encoding="utf-8", newline="\n") as fm:
                fm.write(meta)

    print(f"Generated {len(sources)} x {len(EFFECTS)} = {len(sources)*len(EFFECTS)} assets from {len(sources)} base cards.")


if __name__ == "__main__":
    main()
