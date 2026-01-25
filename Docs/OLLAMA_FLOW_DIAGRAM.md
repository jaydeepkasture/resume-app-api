# Ollama Request Flow - Visual Guide

## Request Processing Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    User Sends Enhancement Request                │
│                                                                   │
│  Examples:                                                        │
│  • "Add icons next to skills"                                    │
│  • "Improve my work experience"                                  │
│  • "Add icons to skills AND enhance my summary"                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│              ResumeRepository.ChatEnhanceAsync()                 │
│                                                                   │
│  • Receives: message, chatId, resumeHtml                         │
│  • Retrieves current resume data from MongoDB                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│         OllamaService.EnhanceResumeHtmlAsync()                   │
│                                                                   │
│  • Calls BuildHtmlEnhancementPrompt()                            │
│  • Sends to Ollama API                                           │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│         BuildHtmlEnhancementPrompt() - THE KEY METHOD            │
│                                                                   │
│  Creates a single unified prompt with:                           │
│  ✓ Current HTML                                                  │
│  ✓ Current JSON                                                  │
│  ✓ User's message                                                │
│  ✓ Clear instructions on when to update JSON                    │
│  ✓ Examples of design vs content vs mixed requests              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Ollama (deepseek-r1:8b)                       │
│                                                                   │
│  Analyzes the request and decides:                               │
│  • Is this design-only? → Update HTML only                       │
│  • Is this content-only? → Update HTML + JSON                    │
│  • Is this mixed? → Update HTML for both, JSON for content only  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Ollama Response                             │
│                                                                   │
│  ===HTML_START===                                                │
│  <enhanced HTML with all changes>                                │
│  ===HTML_END===                                                  │
│                                                                   │
│  ===JSON_START===                                                │
│  { "updated JSON if content changed" }                           │
│  ===JSON_END===                                                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│      ParseHtmlAndResumeFromResponse()                            │
│                                                                   │
│  • Extracts HTML between markers                                 │
│  • Extracts JSON between markers                                 │
│  • Returns tuple (enhancedHtml, enhancedResume)                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│              Save to MongoDB (ResumeEnhancementHistory)          │
│                                                                   │
│  • resumeHtml: Enhanced HTML                                     │
│  • enhancedResume: Enhanced JSON                                 │
│  • userMessage: Original request                                 │
│  • chatId: Conversation ID                                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Return to Frontend                            │
│                                                                   │
│  {                                                                │
│    "enhancedHtml": "...",                                        │
│    "enhancedResume": { ... },                                    │
│    "historyId": "..."                                            │
│  }                                                                │
└─────────────────────────────────────────────────────────────────┘
```

---

## Decision Tree: How Ollama Decides

```
                    User Request Received
                            │
                            ↓
              ┌─────────────────────────┐
              │  Ollama Analyzes Request │
              └─────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ↓                   ↓                   ↓
   Design Only         Content Only         Mixed Request
   (e.g., icons)       (e.g., improve)      (e.g., both)
        │                   │                   │
        ↓                   ↓                   ↓
  ┌──────────┐        ┌──────────┐        ┌──────────┐
  │ HTML: ✓  │        │ HTML: ✓  │        │ HTML: ✓  │
  │ JSON: ✗  │        │ JSON: ✓  │        │ JSON: ⚠  │
  └──────────┘        └──────────┘        └──────────┘
       │                   │                   │
       ↓                   ↓                   ↓
  Add visual         Update both         HTML: All changes
  elements only      with enhanced       JSON: Content only
                     content
```

---

## Example Scenarios

### Scenario 1: Design Only Request

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT                                                        │
├─────────────────────────────────────────────────────────────┤
│ Message: "Add icons next to each skill"                     │
│ HTML: <ul><li>C#</li><li>Python</li></ul>                   │
│ JSON: { "skills": ["C#", "Python"] }                        │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OLLAMA DECISION                                              │
├─────────────────────────────────────────────────────────────┤
│ This is a DESIGN request (visual change only)               │
│ → Update HTML with icons                                    │
│ → Keep JSON unchanged                                       │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT                                                       │
├─────────────────────────────────────────────────────────────┤
│ HTML: <ul>                                                   │
│         <li>                                                 │
│           <img src="cdn.simpleicons.org/csharp"/> C#         │
│         </li>                                                │
│         <li>                                                 │
│           <img src="cdn.simpleicons.org/python"/> Python     │
│         </li>                                                │
│       </ul>                                                  │
│                                                              │
│ JSON: { "skills": ["C#", "Python"] }  ← UNCHANGED           │
└─────────────────────────────────────────────────────────────┘
```

---

### Scenario 2: Content Only Request

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT                                                        │
├─────────────────────────────────────────────────────────────┤
│ Message: "Improve my summary with achievements"             │
│ HTML: <p>Experienced developer</p>                          │
│ JSON: { "summary": "Experienced developer" }                │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OLLAMA DECISION                                              │
├─────────────────────────────────────────────────────────────┤
│ This is a CONTENT request (text improvement)                │
│ → Update HTML with enhanced text                            │
│ → Update JSON with enhanced text                            │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT                                                       │
├─────────────────────────────────────────────────────────────┤
│ HTML: <p>Senior Software Engineer with 5+ years of          │
│          experience delivering scalable solutions</p>        │
│                                                              │
│ JSON: {                                                      │
│   "summary": "Senior Software Engineer with 5+ years..."    │
│ }  ← UPDATED                                                │
└─────────────────────────────────────────────────────────────┘
```

---

### Scenario 3: Mixed Request

```
┌─────────────────────────────────────────────────────────────┐
│ INPUT                                                        │
├─────────────────────────────────────────────────────────────┤
│ Message: "Add icons to skills AND improve my summary"       │
│ HTML: <div>                                                  │
│         <p>Developer</p>                                     │
│         <ul><li>C#</li></ul>                                 │
│       </div>                                                 │
│ JSON: {                                                      │
│   "summary": "Developer",                                    │
│   "skills": ["C#"]                                           │
│ }                                                            │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OLLAMA DECISION                                              │
├─────────────────────────────────────────────────────────────┤
│ This is a MIXED request                                     │
│ → Icons = DESIGN → Update HTML only                         │
│ → Summary = CONTENT → Update HTML + JSON                    │
└─────────────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ OUTPUT                                                       │
├─────────────────────────────────────────────────────────────┤
│ HTML: <div>                                                  │
│         <p>Senior Developer with 5+ years...</p>             │
│         <ul>                                                 │
│           <li>                                               │
│             <img src="cdn.simpleicons.org/csharp"/> C#       │
│           </li>                                              │
│         </ul>                                                │
│       </div>                                                 │
│                                                              │
│ JSON: {                                                      │
│   "summary": "Senior Developer with 5+ years...",  ← UPDATED │
│   "skills": ["C#"]  ← UNCHANGED (icons are design)          │
│ }                                                            │
└─────────────────────────────────────────────────────────────┘
```

---

## Key Takeaways

1. **No Hardcoded Logic** - Ollama makes intelligent decisions based on the prompt
2. **Handles Any Combination** - Design, content, or mixed requests
3. **JSON Integrity** - Only updates when actual content changes
4. **Natural Language** - Users can phrase requests however they want
5. **Examples Guide AI** - The prompt includes examples to teach Ollama

---

## Monitoring Tips

✅ Check the response HTML for all requested changes  
✅ Compare input JSON vs output JSON to verify correct updates  
✅ For design requests, JSON should be identical  
✅ For content requests, JSON should reflect the improvements  
✅ For mixed requests, JSON should only have content changes
