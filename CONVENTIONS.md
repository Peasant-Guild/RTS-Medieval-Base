# General Project Conventions
This file defines the shared conventions for this RTS-Medieval-Base game project.
Hopefully all devs and future contributers will follow these conventions, or else...

## General Rules
- When possible, keep things simple and readable.
- Do not make large unrelated changes in one commit.
- Before submitting PR's, compile and run the project.
- Try to force at least one other dev to go over your PR.

## Folder Structure
Use this structure inside `Assets/`:
Assets/
+---Art/
+---Audio/
+---Code/
| +---Scripts/
+---Docs/
+---Level/
| +---Prefabs/
| +---Scenes/
| +---UI/

## C# Naming

### General
- Generally try to avoid long and non-indicative naming.
- The Short and Inidicative party Wants you! #Peace

### Files and classes
- Use `PascalCase` for class names and file names.
- For all of the above avoid long andand summarize the file's

 For Example:
 - `ActorController.cs`

### Variables
- Public fields: `camelCase`
- Private fields: `_camelCase`
 For Example:
 - `[SerializeField] private float _prefabsObject`

### Methods
- `PascalCase`

### GameObjects
- `PascalCase`

### Prefabs
- `PascalCase`
- If needed, add category prefix like UI_prefabName

### Scenes
- `PascalCase` or numbered names if in need.

## Scripts
- Try to keep one major class per file.
- Generally single responsability is the best tell of coding ability :)
- Functions and methods, should generally not exceed 60 lines, and never more than 100 lines, if i'll see one in a PR 
  remember.. I know where you live.  
- Docstrings are recommended.
- Otherwise comments should be left on areas of the code that contain, complex or hard to 
  understand calculations, and other edge cases.
- No vibe coding
- If vibe coding, do it sneakly (and dont leave the LLM's stupid comments)


## Git
- Always remember to pull before working, This convention was written in blood!
- Make sure commit messages are clear.
- Please, for the love of god, make sure you are the only person working on your branch.
- Please, for the love of god, don't work on another person's branch without their knowledge of it.
- Branch names should follow this format:
  [feature/fix/etc...]/branch-name-with-hyphens

  For example:
    feature/project-feature-name
    fix/jump-bug


