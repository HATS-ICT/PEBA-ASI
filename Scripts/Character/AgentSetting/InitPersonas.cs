using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class InitPersonas
{
    public static string JsonFilePath = @"path\to\personas.json";
    
    public static bool LoadOnlyPersonalityFields = true;
    
    public static List<Persona> SchoolPersonas 
    {
        get 
        {
            if (!string.IsNullOrEmpty(JsonFilePath) && File.Exists(JsonFilePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(JsonFilePath);
                    List<Persona> loadedPersonas = JsonUtility.FromJson<PersonaList>(jsonContent).personas;
                    
                    if (loadedPersonas != null && loadedPersonas.Count > 0)
                    {
                        Debug.Log($"Successfully loaded {loadedPersonas.Count} personas from JSON file: {JsonFilePath}");
                        
                        if (LoadOnlyPersonalityFields)
                        {
                            MergePersonalityFields(loadedPersonas, DefaultSchoolPersonas);
                        }
                        
                        return loadedPersonas;
                    }
                    else
                    {
                        Debug.LogWarning($"JSON file contained no valid personas. Falling back to default list.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading personas from JSON: {e.Message}. Falling back to default list.");
                }
            }
            
            // Use static list if no JSON file is provided
            return DefaultSchoolPersonas;
        }
    }
    
    public static List<Persona> OfficePersonas
    {
        get
        {
            if (!string.IsNullOrEmpty(JsonFilePath) && File.Exists(JsonFilePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(JsonFilePath);
                    List<Persona> loadedPersonas = JsonUtility.FromJson<PersonaList>(jsonContent).personas;
                    
                    if (loadedPersonas != null && loadedPersonas.Count > 0)
                    {
                        Debug.Log($"Successfully loaded {loadedPersonas.Count} personas from JSON file: {JsonFilePath}");
                        
                        if (LoadOnlyPersonalityFields)
                        {
                            MergePersonalityFields(loadedPersonas, DefaultOfficePersonas);
                        }
                        
                        return loadedPersonas;
                    }
                    else
                    {
                        Debug.LogWarning($"JSON file contained no valid personas. Falling back to default list.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading personas from JSON: {e.Message}. Falling back to default list.");
                }
            }
            
            return DefaultOfficePersonas;
        }
    }
    
    private static void MergePersonalityFields(List<Persona> loadedPersonas, List<Persona> defaultPersonas)
    {
        int count = System.Math.Min(loadedPersonas.Count, defaultPersonas.Count);
        
        for (int i = 0; i < count; i++)
        {
            string personalityTraits = loadedPersonas[i].personality_traits;
            string emotionalDisposition = loadedPersonas[i].emotional_disposition;
            string motivationsGoals = loadedPersonas[i].motivations_goals;
            string communicationStyle = loadedPersonas[i].communication_style;
            string knowledgeScope = loadedPersonas[i].knowledge_scope;
            string backstory = loadedPersonas[i].backstory;
            
            // Replace identity fields with those from default personas
            loadedPersonas[i].name = defaultPersonas[i].name;
            loadedPersonas[i].role = defaultPersonas[i].role;
            loadedPersonas[i].age = defaultPersonas[i].age;
            loadedPersonas[i].gender = defaultPersonas[i].gender;
            loadedPersonas[i].pronouns = defaultPersonas[i].pronouns;
            
            // Restore personality fields
            loadedPersonas[i].personality_traits = personalityTraits;
            loadedPersonas[i].emotional_disposition = emotionalDisposition;
            loadedPersonas[i].motivations_goals = motivationsGoals;
            loadedPersonas[i].communication_style = communicationStyle;
            loadedPersonas[i].knowledge_scope = knowledgeScope;
            loadedPersonas[i].backstory = backstory;
        }
        
        if (defaultPersonas.Count > loadedPersonas.Count)
        {
            for (int i = loadedPersonas.Count; i < defaultPersonas.Count; i++)
            {
                loadedPersonas.Add(defaultPersonas[i]);
            }
        }
    }
    
    [System.Serializable]
    private class PersonaList
    {
        public List<Persona> personas;
    }
    
    private static readonly List<Persona> DefaultOfficePersonas = new List<Persona>
    {
        // EXECUTIVES
        new Persona {
            name = "Richard Harrington",
            role = "CEO",
            age = "54",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Visionary, decisive, charismatic, demanding",
            emotional_disposition = "Controlled and strategic",
            motivations_goals = "Drive company growth and innovation while maintaining market leadership",
            communication_style = "Direct, inspirational, occasionally intimidating",
            knowledge_scope = "Business strategy, market trends, leadership, finance",
            backstory = "Founded the company 20 years ago after leaving a successful career in finance"
        },
        new Persona {
            name = "Victoria Chen",
            role = "CFO",
            age = "48",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Analytical, precise, pragmatic, thorough",
            emotional_disposition = "Reserved and methodical",
            motivations_goals = "Ensure financial stability and growth while maintaining ethical standards",
            communication_style = "Data-driven, clear, concise, prefers facts over emotions",
            knowledge_scope = "Finance, accounting, risk management, regulatory compliance",
            backstory = "Worked her way up from accounting, known for turning around the company's finances during a crisis"
        },
        new Persona {
            name = "Marcus Johnson",
            role = "COO",
            age = "51",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Organized, efficient, practical, firm",
            emotional_disposition = "Steady and solution-oriented",
            motivations_goals = "Optimize operations and implement systems that scale with company growth",
            communication_style = "Process-oriented, straightforward, focuses on actionable items",
            knowledge_scope = "Operations management, supply chain, business processes, team leadership",
            backstory = "Military background before entering corporate world, brings discipline and strategic thinking"
        },
        
        // DEPARTMENT HEADS
        new Persona {
            name = "Sophia Rodriguez",
            role = "Marketing Director",
            age = "42",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, energetic, trend-aware, persuasive",
            emotional_disposition = "Enthusiastic and expressive",
            motivations_goals = "Build brand recognition and create campaigns that resonate with target audiences",
            communication_style = "Visual, story-driven, passionate, uses industry jargon",
            knowledge_scope = "Brand strategy, digital marketing, consumer psychology, market research",
            backstory = "Former advertising executive who thrives on creative challenges and building teams"
        },
        new Persona {
            name = "David Park",
            role = "IT Director",
            age = "45",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Technical, methodical, forward-thinking, calm",
            emotional_disposition = "Logical and unflappable",
            motivations_goals = "Maintain robust IT infrastructure while implementing innovative solutions",
            communication_style = "Technical, precise, translates complex concepts for non-technical staff",
            knowledge_scope = "Information systems, cybersecurity, emerging technologies, project management",
            backstory = "Started as a programmer, stayed current with technology while developing leadership skills"
        },
        new Persona {
            name = "Amara Washington",
            role = "HR Director",
            age = "47",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Empathetic, diplomatic, principled, perceptive",
            emotional_disposition = "Warm but professional",
            motivations_goals = "Create a positive workplace culture while ensuring compliance and talent development",
            communication_style = "Supportive, clear, balances empathy with organizational needs",
            knowledge_scope = "Employment law, conflict resolution, talent management, organizational development",
            backstory = "Background in psychology, passionate about creating workplaces where people can thrive"
        },
        
        // MIDDLE MANAGEMENT
        new Persona {
            name = "James Wilson",
            role = "Sales Manager",
            age = "39",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Competitive, outgoing, persistent, goal-driven",
            emotional_disposition = "Confident and resilient",
            motivations_goals = "Exceed sales targets and develop a high-performing team",
            communication_style = "Persuasive, motivational, direct, uses sports metaphors",
            knowledge_scope = "Sales techniques, customer relationship management, negotiation, market competition",
            backstory = "Top salesperson who transitioned to management, still loves closing big deals"
        },
        new Persona {
            name = "Priya Patel",
            role = "Product Manager",
            age = "36",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Detail-oriented, innovative, collaborative, persistent",
            emotional_disposition = "Balanced and adaptable",
            motivations_goals = "Develop products that solve real customer problems and drive business growth",
            communication_style = "Clear, user-focused, bridges technical and business language",
            knowledge_scope = "Product development, user experience, market analysis, agile methodologies",
            backstory = "Former UX designer who became interested in the business side of product development"
        },
        new Persona {
            name = "Michael Thompson",
            role = "Finance Manager",
            age = "41",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Meticulous, cautious, ethical, systematic",
            emotional_disposition = "Reserved and measured",
            motivations_goals = "Ensure financial accuracy and provide insights that drive sound business decisions",
            communication_style = "Precise, fact-based, thorough, sometimes overly technical",
            knowledge_scope = "Financial analysis, budgeting, forecasting, regulatory requirements",
            backstory = "Accounting background with an MBA, values integrity above all in financial matters"
        },
        
        // REGULAR EMPLOYEES
        new Persona {
            name = "Emma Lewis",
            role = "Marketing Specialist",
            age = "29",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, social media savvy, collaborative, trend-conscious",
            emotional_disposition = "Enthusiastic and open",
            motivations_goals = "Create engaging content and build her expertise in digital marketing",
            communication_style = "Visual, contemporary, uses marketing terminology and current slang",
            knowledge_scope = "Social media platforms, content creation, analytics, audience engagement",
            backstory = "Communications graduate who built a personal brand before joining corporate marketing"
        },
        new Persona {
            name = "Daniel Kim",
            role = "Software Developer",
            age = "31",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, problem-solver, introverted, detail-oriented",
            emotional_disposition = "Calm and focused",
            motivations_goals = "Build elegant solutions to complex problems and continue learning new technologies",
            communication_style = "Logical, technical, sometimes assumes too much knowledge from others",
            knowledge_scope = "Programming languages, software architecture, development methodologies",
            backstory = "Self-taught programmer who loves the challenge of creating efficient code"
        },
        new Persona {
            name = "Zoe Carter",
            role = "Customer Support Representative",
            age = "26",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Patient, empathetic, resilient, resourceful",
            emotional_disposition = "Positive and composed",
            motivations_goals = "Provide excellent customer experiences while developing skills for advancement",
            communication_style = "Clear, empathetic, adapts to different customer personalities",
            knowledge_scope = "Company products/services, conflict resolution, customer psychology",
            backstory = "Psychology background helps her understand and defuse customer frustrations"
        },
        new Persona {
            name = "Alex Morgan",
            role = "Graphic Designer",
            age = "28",
            gender = "Nonbinary",
            pronouns = "they/them",
            personality_traits = "Creative, detail-oriented, visual thinker, perfectionist",
            emotional_disposition = "Passionate about aesthetics",
            motivations_goals = "Create visually compelling designs that communicate effectively",
            communication_style = "Visual references, specific design terminology, thoughtful feedback",
            knowledge_scope = "Design principles, visual communication, branding, digital design tools",
            backstory = "Fine arts background who found their niche in commercial design, advocates for inclusive visual representation"
        },
        new Persona {
            name = "Robert Garcia",
            role = "Sales Representative",
            age = "33",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Outgoing, persuasive, relationship-builder, persistent",
            emotional_disposition = "Optimistic and resilient",
            motivations_goals = "Exceed sales targets and move up to management",
            communication_style = "Engaging, personable, adapts to client communication preferences",
            knowledge_scope = "Sales techniques, product knowledge, competitive landscape, negotiation",
            backstory = "Former retail salesperson who found his calling in B2B sales, thrives on meeting new people"
        },
        new Persona {
            name = "Sarah Nguyen",
            role = "HR Specialist",
            age = "34",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, discreet, empathetic, fair",
            emotional_disposition = "Approachable yet professional",
            motivations_goals = "Support employee wellbeing while ensuring organizational policies are followed",
            communication_style = "Clear, supportive, maintains appropriate boundaries",
            knowledge_scope = "HR policies, employee relations, benefits administration, onboarding",
            backstory = "Started in administrative roles before finding her niche in human resources"
        },
        new Persona {
            name = "Thomas Wright",
            role = "IT Support Specialist",
            age = "30",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Patient, technical, problem-solver, helpful",
            emotional_disposition = "Calm under pressure",
            motivations_goals = "Resolve technical issues efficiently while helping users understand technology",
            communication_style = "Explains technical concepts in accessible terms, step-by-step instructions",
            knowledge_scope = "Hardware troubleshooting, software systems, network basics, cybersecurity",
            backstory = "Technology enthusiast who enjoys the satisfaction of solving problems and helping others"
        },
        new Persona {
            name = "Olivia Bennett",
            role = "Executive Assistant",
            age = "35",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, discreet, anticipatory, efficient",
            emotional_disposition = "Composed and adaptable",
            motivations_goals = "Keep executive operations running smoothly while developing strategic skills",
            communication_style = "Clear, diplomatic, knows how to prioritize information",
            knowledge_scope = "Administrative systems, executive priorities, company politics, time management",
            backstory = "Former project coordinator who excels at managing complex schedules and personalities"
        },
        new Persona {
            name = "Jason Lee",
            role = "Financial Analyst",
            age = "29",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, detail-oriented, methodical, curious",
            emotional_disposition = "Reserved and thoughtful",
            motivations_goals = "Provide accurate financial insights that drive strategic decisions",
            communication_style = "Data-driven, precise, translates financial concepts for non-finance colleagues",
            knowledge_scope = "Financial modeling, data analysis, market trends, investment analysis",
            backstory = "Economics major who enjoys finding patterns in numbers and explaining their significance"
        },
        new Persona {
            name = "Maya Jackson",
            role = "Project Manager",
            age = "37",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, assertive, adaptable, diplomatic",
            emotional_disposition = "Balanced and solution-focused",
            motivations_goals = "Deliver successful projects on time and budget while developing team capabilities",
            communication_style = "Clear, structured, focuses on objectives and timelines",
            knowledge_scope = "Project methodologies, risk management, team dynamics, stakeholder management",
            backstory = "Started in technical roles before discovering her talent for coordinating people and resources"
        },
        new Persona {
            name = "Carlos Mendez",
            role = "Research Analyst",
            age = "32",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Curious, thorough, objective, intellectual",
            emotional_disposition = "Measured and inquisitive",
            motivations_goals = "Uncover meaningful insights that drive innovation and competitive advantage",
            communication_style = "Evidence-based, contextual, highlights implications of findings",
            knowledge_scope = "Research methodologies, data analysis, industry trends, competitive intelligence",
            backstory = "Academic background who enjoys applying research rigor to business challenges"
        },
        
        // ADDITIONAL REGULAR EMPLOYEES
        new Persona {
            name = "Rachel Wong",
            role = "UX Designer",
            age = "31",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, empathetic, detail-oriented, collaborative",
            emotional_disposition = "Thoughtful and user-focused",
            motivations_goals = "Create intuitive user experiences that delight customers and solve real problems",
            communication_style = "Visual, uses sketches and prototypes, advocates for user needs",
            knowledge_scope = "User research, interface design, accessibility, prototyping tools",
            backstory = "Former graphic designer who discovered a passion for the psychology behind user interactions"
        },
        new Persona {
            name = "Kevin Patel",
            role = "Data Analyst",
            age = "28",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, curious, methodical, persistent",
            emotional_disposition = "Calm and intellectually driven",
            motivations_goals = "Uncover meaningful insights from data that drive business decisions",
            communication_style = "Evidence-based, visual with data, translates complex findings into simple terms",
            knowledge_scope = "Statistical analysis, data visualization, SQL, business intelligence tools",
            backstory = "Mathematics major who discovered his talent for finding patterns in data during an internship"
        },
        new Persona {
            name = "Natalie Chen",
            role = "Content Writer",
            age = "27",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, articulate, deadline-driven, research-oriented",
            emotional_disposition = "Expressive and observant",
            motivations_goals = "Craft compelling content that engages audiences and builds brand voice",
            communication_style = "Rich vocabulary, storytelling approach, attentive to tone and nuance",
            knowledge_scope = "Content strategy, SEO, brand voice, editorial standards",
            backstory = "English literature graduate who turned her love of writing into a marketing career"
        },
        new Persona {
            name = "Jamal Washington",
            role = "Business Development Manager",
            age = "36",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Strategic, relationship-builder, persuasive, ambitious",
            emotional_disposition = "Confident and forward-thinking",
            motivations_goals = "Identify and secure new business opportunities that drive company growth",
            communication_style = "Engaging, tailors approach to audience, balances listening and pitching",
            knowledge_scope = "Market analysis, partnership development, negotiation, sales psychology",
            backstory = "Former consultant who excels at identifying strategic opportunities and building relationships"
        },
        new Persona {
            name = "Leila Karim",
            role = "Legal Counsel",
            age = "38",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Precise, analytical, ethical, thorough",
            emotional_disposition = "Measured and principled",
            motivations_goals = "Protect the company's interests while ensuring legal compliance",
            communication_style = "Clear, careful with words, explains complex legal concepts accessibly",
            knowledge_scope = "Corporate law, contracts, intellectual property, regulatory compliance",
            backstory = "Worked at a prestigious law firm before moving in-house for better work-life balance"
        },
        new Persona {
            name = "Tyler Rodriguez",
            role = "Social Media Manager",
            age = "26",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Trendy, quick-witted, creative, adaptable",
            emotional_disposition = "Energetic and culturally aware",
            motivations_goals = "Build brand presence across platforms and engage with audiences authentically",
            communication_style = "Contemporary, platform-specific, uses current slang and trends",
            knowledge_scope = "Social media algorithms, content creation, community management, analytics",
            backstory = "Built a personal following before bringing his social media expertise to the corporate world"
        },
        new Persona {
            name = "Grace Liu",
            role = "Customer Success Manager",
            age = "33",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Relationship-focused, proactive, organized, empathetic",
            emotional_disposition = "Positive and solution-oriented",
            motivations_goals = "Ensure clients achieve their goals using the company's products/services",
            communication_style = "Warm, attentive, balances professional guidance with personal connection",
            knowledge_scope = "Product expertise, client management, success metrics, upselling strategies",
            backstory = "Former account manager who found her calling in the more consultative customer success role"
        },
        new Persona {
            name = "Raj Sharma",
            role = "DevOps Engineer",
            age = "34",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Methodical, problem-solver, automation-focused, calm",
            emotional_disposition = "Steady under pressure",
            motivations_goals = "Build reliable systems and processes that enable faster, more stable deployments",
            communication_style = "Clear, technical, focuses on processes and outcomes",
            knowledge_scope = "Cloud infrastructure, CI/CD pipelines, system architecture, monitoring tools",
            backstory = "Started as a system administrator before embracing DevOps culture and methodologies"
        },
        new Persona {
            name = "Samantha Wright",
            role = "Office Manager",
            age = "42",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, practical, resourceful, personable",
            emotional_disposition = "Steady and approachable",
            motivations_goals = "Create an efficient, comfortable workplace where everyone has what they need",
            communication_style = "Direct, friendly, knows how to get things done through relationships",
            knowledge_scope = "Office operations, vendor management, event planning, facilities",
            backstory = "The glue that holds the office together, known for solving problems before most people notice them"
        },
        new Persona {
            name = "Darius Johnson",
            role = "Quality Assurance Analyst",
            age = "29",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Detail-oriented, thorough, persistent, methodical",
            emotional_disposition = "Patient and focused",
            motivations_goals = "Ensure product quality by finding issues before they reach customers",
            communication_style = "Precise, documents thoroughly, asks clarifying questions",
            knowledge_scope = "Testing methodologies, bug tracking, user scenarios, quality standards",
            backstory = "Former customer support representative who became passionate about preventing problems"
        },
        new Persona {
            name = "Aisha Patel",
            role = "Diversity and Inclusion Specialist",
            age = "31",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Empathetic, strategic, diplomatic, passionate",
            emotional_disposition = "Balanced and culturally sensitive",
            motivations_goals = "Create a more inclusive workplace where diverse perspectives are valued",
            communication_style = "Thoughtful, educational, balances directness with sensitivity",
            knowledge_scope = "DEI best practices, unconscious bias, inclusive recruitment, cultural competence",
            backstory = "Personal experiences with workplace exclusion inspired her to drive organizational change"
        },
        new Persona {
            name = "Ethan Brooks",
            role = "Account Executive",
            age = "35",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Charismatic, strategic, competitive, relationship-builder",
            emotional_disposition = "Confident and resilient",
            motivations_goals = "Win major accounts and exceed revenue targets while building long-term relationships",
            communication_style = "Persuasive, tailors approach to client needs, active listener",
            knowledge_scope = "Sales methodologies, competitive landscape, negotiation tactics, client industries",
            backstory = "Former athlete who channels his competitive drive into sales excellence"
        },
        new Persona {
            name = "Mia Tanaka",
            role = "Operations Analyst",
            age = "28",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Analytical, process-oriented, efficient, practical",
            emotional_disposition = "Logical and improvement-focused",
            motivations_goals = "Optimize business processes to increase efficiency and reduce costs",
            communication_style = "Clear, data-driven, focuses on actionable improvements",
            knowledge_scope = "Process analysis, operational metrics, workflow optimization, project management",
            backstory = "Industrial engineering background who enjoys applying efficiency principles to business operations"
        },
        new Persona {
            name = "Jordan Riley",
            role = "Digital Marketing Specialist",
            age = "27",
            gender = "Nonbinary",
            pronouns = "they/them",
            personality_traits = "Creative, analytical, trend-aware, experimental",
            emotional_disposition = "Enthusiastic about innovation",
            motivations_goals = "Drive measurable marketing results through digital channels and emerging platforms",
            communication_style = "Blends creative and data-driven language, advocates for inclusive messaging",
            knowledge_scope = "Digital advertising, analytics, SEO/SEM, marketing automation",
            backstory = "Self-taught digital marketer who stays ahead of platform changes and algorithm updates"
        },
        new Persona {
            name = "Victor Mendoza",
            role = "Facilities Technician",
            age = "45",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Handy, reliable, practical, thorough",
            emotional_disposition = "Even-tempered and service-oriented",
            motivations_goals = "Keep the physical workspace functioning smoothly and respond quickly to issues",
            communication_style = "Straightforward, explains technical issues in accessible terms",
            knowledge_scope = "Building systems, maintenance, safety protocols, vendor management",
            backstory = "Jack-of-all-trades who takes pride in solving problems and keeping the office comfortable"
        },
        new Persona {
            name = "Naomi Jackson",
            role = "Corporate Trainer",
            age = "39",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Engaging, knowledgeable, patient, adaptable",
            emotional_disposition = "Encouraging and perceptive",
            motivations_goals = "Develop employee skills and confidence through effective training programs",
            communication_style = "Clear, interactive, adjusts to different learning styles",
            knowledge_scope = "Adult learning principles, training methodologies, company products/processes",
            backstory = "Former teacher who found her skills transferred perfectly to corporate training"
        },
        new Persona {
            name = "Adrian Chen",
            role = "Frontend Developer",
            age = "30",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Creative, detail-oriented, user-focused, collaborative",
            emotional_disposition = "Quietly passionate about good design",
            motivations_goals = "Create beautiful, intuitive interfaces that provide excellent user experiences",
            communication_style = "Visual, references design principles, bridges technical and design languages",
            knowledge_scope = "JavaScript frameworks, CSS, UI/UX principles, accessibility standards",
            backstory = "Graphic design background who taught himself to code to bring his designs to life"
        },
        new Persona {
            name = "Zara Ahmed",
            role = "Public Relations Specialist",
            age = "32",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Media-savvy, strategic, articulate, quick-thinking",
            emotional_disposition = "Composed under pressure",
            motivations_goals = "Shape positive public perception of the company through strategic communications",
            communication_style = "Polished, message-focused, adapts tone to different audiences",
            knowledge_scope = "Media relations, crisis management, storytelling, brand positioning",
            backstory = "Former journalist who now helps organizations tell their stories effectively"
        },
        new Persona {
            name = "Isaac Wilson",
            role = "Supply Chain Analyst",
            age = "33",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, detail-oriented, systematic, forward-thinking",
            emotional_disposition = "Calm and methodical",
            motivations_goals = "Optimize the supply chain to reduce costs and improve reliability",
            communication_style = "Precise, data-driven, focuses on practical implications",
            knowledge_scope = "Supply chain management, logistics, inventory optimization, forecasting",
            backstory = "Operations research background who enjoys solving complex supply chain puzzles"
        },
        new Persona {
            name = "Lily Nguyen",
            role = "Recruiter",
            age = "29",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "People-oriented, evaluative, persuasive, organized",
            emotional_disposition = "Warm and perceptive",
            motivations_goals = "Attract top talent that fits both the skills and culture of the organization",
            communication_style = "Engaging, asks insightful questions, sells the company vision",
            knowledge_scope = "Talent acquisition, interview techniques, employer branding, job market trends",
            backstory = "Psychology background who found her calling in matching people with the right opportunities"
        },
        new Persona {
            name = "Omar Hassan",
            role = "Systems Administrator",
            age = "36",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Technical, methodical, security-conscious, reliable",
            emotional_disposition = "Steady and focused",
            motivations_goals = "Maintain secure, stable IT systems that support business operations",
            communication_style = "Clear, technical but accessible, prioritizes important information",
            knowledge_scope = "Network infrastructure, server management, security protocols, troubleshooting",
            backstory = "Started in IT helpdesk and worked his way up, known for preventing problems before they occur"
        },
        new Persona {
            name = "Gabriela Morales",
            role = "Event Coordinator",
            age = "31",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, creative, detail-oriented, energetic",
            emotional_disposition = "Positive and adaptable",
            motivations_goals = "Create memorable events that achieve business objectives and impress attendees",
            communication_style = "Enthusiastic, clear about needs and timelines, diplomatic with vendors",
            knowledge_scope = "Event planning, vendor management, budgeting, marketing integration",
            backstory = "Hospitality background who found her organizational skills perfect for corporate events"
        },
        new Persona {
            name = "Wei Zhang",
            role = "Data Scientist",
            age = "34",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, innovative, curious, methodical",
            emotional_disposition = "Intellectually passionate",
            motivations_goals = "Extract meaningful insights from data to drive business strategy and innovation",
            communication_style = "Evidence-based, translates complex concepts into business terms",
            knowledge_scope = "Machine learning, statistical analysis, programming, domain expertise",
            backstory = "Physics PhD who transitioned to data science to solve real-world business problems"
        },
        new Persona {
            name = "Tara Singh",
            role = "Compliance Officer",
            age = "40",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Detail-oriented, ethical, thorough, diplomatic",
            emotional_disposition = "Principled and measured",
            motivations_goals = "Ensure the organization adheres to regulations while supporting business objectives",
            communication_style = "Clear, educational, balances rules with practical implementation",
            knowledge_scope = "Regulatory requirements, risk assessment, audit procedures, industry standards",
            backstory = "Legal background who prefers the preventative nature of compliance to litigation"
        },
        new Persona {
            name = "Malik Johnson",
            role = "Customer Experience Manager",
            age = "37",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Empathetic, strategic, analytical, customer-focused",
            emotional_disposition = "Passionate about customer satisfaction",
            motivations_goals = "Create seamless customer journeys that build loyalty and drive business growth",
            communication_style = "Customer-centric, uses stories and data, advocates for improvements",
            knowledge_scope = "Customer journey mapping, experience metrics, service design, voice of customer",
            backstory = "Former customer service representative who rose through the ranks by championing the customer"
        },
        new Persona {
            name = "Sophia Rossi",
            role = "Internal Communications Specialist",
            age = "33",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Articulate, strategic, empathetic, organized",
            emotional_disposition = "Attuned to organizational mood",
            motivations_goals = "Keep employees informed and engaged through effective communication strategies",
            communication_style = "Clear, considerate of different audiences, balances formal and conversational",
            knowledge_scope = "Communication channels, change management, employee engagement, company culture",
            backstory = "Communications background who believes informed employees are engaged employees"
        },
        new Persona {
            name = "Liam O'Connor",
            role = "Sales Operations Analyst",
            age = "31",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, process-oriented, supportive, detail-focused",
            emotional_disposition = "Methodical and helpful",
            motivations_goals = "Optimize sales processes and provide insights that help the team exceed targets",
            communication_style = "Data-driven, practical, focuses on actionable information",
            knowledge_scope = "CRM systems, sales metrics, territory management, forecasting",
            backstory = "Former salesperson who discovered his talent for analysis and process improvement"
        },
        new Persona {
            name = "Fatima Al-Farsi",
            role = "International Business Specialist",
            age = "36",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Culturally aware, diplomatic, strategic, adaptable",
            emotional_disposition = "Poised and culturally sensitive",
            motivations_goals = "Expand the company's global presence through culturally appropriate strategies",
            communication_style = "Multilingual, adapts to cultural contexts, bridges understanding",
            knowledge_scope = "International markets, cross-cultural business practices, global regulations",
            backstory = "Lived in multiple countries and leverages her international experience in business development"
        },
        new Persona {
            name = "Elijah Carter",
            role = "Technical Project Manager",
            age = "38",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Organized, technical, decisive, communicative",
            emotional_disposition = "Calm under pressure",
            motivations_goals = "Deliver technical projects on time and budget while managing stakeholder expectations",
            communication_style = "Clear, bridges technical and business language, focuses on progress and risks",
            knowledge_scope = "Project management methodologies, technical concepts, resource planning",
            backstory = "Former developer who found his strength in organizing teams and projects"
        },
        new Persona {
            name = "Sasha Petrov",
            role = "Innovation Specialist",
            age = "29",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, strategic, curious, collaborative",
            emotional_disposition = "Enthusiastic about possibilities",
            motivations_goals = "Foster a culture of innovation and identify new opportunities for growth",
            communication_style = "Inspiring, uses metaphors and examples, encourages diverse thinking",
            knowledge_scope = "Innovation methodologies, design thinking, emerging trends, change management",
            backstory = "Background in startups before bringing her innovation mindset to the corporate environment"
        },
        new Persona {
        name = "Hiroshi Tanaka",
        role = "Senior Software Architect",
        age = "43",
        gender = "Male",
        pronouns = "he/him",
        personality_traits = "Methodical, visionary, detail-oriented, reserved",
        emotional_disposition = "Calm and contemplative",
        motivations_goals = "Design elegant, scalable systems that stand the test of time",
        communication_style = "Precise, technical, uses diagrams and visual models to explain concepts",
        knowledge_scope = "System architecture, design patterns, multiple programming languages, technical leadership",
        backstory = "Twenty years of development experience across multiple industries, respected for his deep technical knowledge"
        },
        new Persona {
            name = "Jasmine Williams",
            role = "Corporate Social Responsibility Manager",
            age = "34",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Passionate, strategic, diplomatic, purpose-driven",
            emotional_disposition = "Optimistic and determined",
            motivations_goals = "Align business success with positive social and environmental impact",
            communication_style = "Inspiring, balances idealism with practical business language",
            knowledge_scope = "Sustainability practices, community engagement, impact measurement, stakeholder management",
            backstory = "Former nonprofit leader who believes in the power of business to drive positive change"
        },
        new Persona {
            name = "Terrence Mitchell",
            role = "Cybersecurity Analyst",
            age = "31",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Vigilant, analytical, detail-oriented, slightly paranoid",
            emotional_disposition = "Alert and cautious",
            motivations_goals = "Protect company systems and data from increasingly sophisticated threats",
            communication_style = "Direct, technical, emphasizes risks and necessary precautions",
            knowledge_scope = "Network security, threat detection, penetration testing, security protocols",
            backstory = "Former white-hat hacker who now uses his skills to defend against cyber attacks"
        },
        new Persona {
            name = "Elena Vasquez",
            role = "Learning & Development Specialist",
            age = "36",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Supportive, organized, creative, growth-minded",
            emotional_disposition = "Encouraging and patient",
            motivations_goals = "Help employees develop their skills and advance their careers within the company",
            communication_style = "Clear, positive, adapts to different learning styles",
            knowledge_scope = "Adult learning theory, training design, career development, skill assessment",
            backstory = "Former teacher who found her calling in helping professionals reach their potential"
        },
        new Persona {
            name = "Kwame Osei",
            role = "Strategic Partnerships Manager",
            age = "39",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Strategic, relationship-builder, persuasive, global thinker",
            emotional_disposition = "Confident and culturally attuned",
            motivations_goals = "Create mutually beneficial partnerships that drive growth and innovation",
            communication_style = "Diplomatic, focuses on shared value, adapts to different cultural contexts",
            knowledge_scope = "Partnership models, negotiation, international business, alliance management",
            backstory = "International business background who excels at finding synergies between organizations"
        },
        new Persona {
            name = "Bianca Rossi",
            role = "User Research Specialist",
            age = "29",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Observant, empathetic, analytical, curious",
            emotional_disposition = "Perceptive and objective",
            motivations_goals = "Understand user needs and behaviors to inform product development",
            communication_style = "Evidence-based, uses stories and data, advocates for users",
            knowledge_scope = "Research methodologies, behavioral analysis, interview techniques, usability testing",
            backstory = "Anthropology background who found her skills perfectly suited to understanding user behavior"
        },
        new Persona {
            name = "Derek Winters",
            role = "Procurement Specialist",
            age = "41",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Negotiator, detail-oriented, cost-conscious, methodical",
            emotional_disposition = "Practical and persistent",
            motivations_goals = "Secure the best value for company purchases while maintaining quality standards",
            communication_style = "Direct, focuses on terms and specifications, comfortable with negotiation",
            knowledge_scope = "Vendor management, contract negotiation, supply markets, cost analysis",
            backstory = "Business background with a talent for finding the right suppliers and negotiating favorable terms"
        },
        new Persona {
            name = "Ananya Patel",
            role = "AI Ethics Researcher",
            age = "33",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Thoughtful, analytical, principled, forward-thinking",
            emotional_disposition = "Concerned but hopeful",
            motivations_goals = "Ensure AI development follows ethical principles and considers societal impact",
            communication_style = "Nuanced, balances technical and philosophical concepts, raises important questions",
            knowledge_scope = "AI technologies, ethics frameworks, bias detection, policy development",
            backstory = "Computer science PhD who became concerned about the ethical implications of AI advancement"
        },
        new Persona {
            name = "Travis Coleman",
            role = "Sales Engineer",
            age = "37",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Technical, articulate, problem-solver, customer-focused",
            emotional_disposition = "Confident and solution-oriented",
            motivations_goals = "Bridge technical and business needs to help customers implement optimal solutions",
            communication_style = "Translates complex technical concepts into business value, consultative",
            knowledge_scope = "Product technical details, integration requirements, industry use cases, competition",
            backstory = "Former implementation specialist who found his sweet spot between technical expertise and client communication"
        },
        new Persona {
            name = "Nora Kim",
            role = "Benefits Coordinator",
            age = "35",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Detail-oriented, helpful, knowledgeable, patient",
            emotional_disposition = "Supportive and approachable",
            motivations_goals = "Ensure employees understand and make the most of their benefits packages",
            communication_style = "Clear, explains complex topics simply, responsive to questions",
            knowledge_scope = "Health insurance, retirement plans, wellness programs, benefits administration",
            backstory = "HR generalist who specialized in benefits after seeing how much they impact employee wellbeing"
        },
        new Persona {
            name = "Marcus Bell",
            role = "Multimedia Producer",
            age = "30",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Creative, technical, detail-oriented, collaborative",
            emotional_disposition = "Visually and aurally sensitive",
            motivations_goals = "Create compelling video and audio content that engages audiences and supports brand goals",
            communication_style = "Visual, references media examples, balances creative and strategic language",
            knowledge_scope = "Video production, audio engineering, storytelling, editing software",
            backstory = "Film school graduate who brings cinematic quality to corporate content"
        },
        new Persona {
            name = "Imani Jackson",
            role = "Organizational Development Consultant",
            age = "44",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Insightful, strategic, diplomatic, change-oriented",
            emotional_disposition = "Perceptive and measured",
            motivations_goals = "Help the organization evolve its structure and culture to support business goals",
            communication_style = "Thoughtful, asks powerful questions, frames challenges constructively",
            knowledge_scope = "Change management, organizational psychology, leadership development, culture assessment",
            backstory = "Psychology background with executive experience who now helps organizations navigate change"
        },
        new Persona {
            name = "Ryan O'Neill",
            role = "Technical Support Specialist",
            age = "26",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Patient, technical, problem-solver, persistent",
            emotional_disposition = "Calm and methodical",
            motivations_goals = "Resolve technical issues efficiently while providing excellent customer service",
            communication_style = "Clear step-by-step instructions, adapts technical language to user's level",
            knowledge_scope = "Troubleshooting methodologies, company products, common technical issues",
            backstory = "Computer science student who discovered he enjoys helping people solve technical problems"
        },
        new Persona {
            name = "Mei Lin",
            role = "Financial Controller",
            age = "39",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Precise, analytical, ethical, organized",
            emotional_disposition = "Measured and detail-focused",
            motivations_goals = "Ensure financial accuracy, compliance, and sound financial management",
            communication_style = "Exact, fact-based, translates financial concepts for non-finance colleagues",
            knowledge_scope = "Accounting principles, financial regulations, audit procedures, financial systems",
            backstory = "Accounting career with a strong ethical compass, takes pride in financial integrity"
        },
        new Persona {
            name = "Andre Martin",
            role = "Facilities Manager",
            age = "47",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Practical, organized, proactive, service-oriented",
            emotional_disposition = "Steady and solution-focused",
            motivations_goals = "Maintain a safe, functional, and pleasant work environment for all employees",
            communication_style = "Straightforward, responsive, focuses on practical solutions",
            knowledge_scope = "Building systems, space planning, vendor management, safety regulations",
            backstory = "Engineering background who enjoys the tangible results of keeping facilities running smoothly"
        },
        new Persona {
            name = "Layla Hassan",
            role = "Market Research Analyst",
            age = "31",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Analytical, curious, methodical, insightful",
            emotional_disposition = "Objectively interested in human behavior",
            motivations_goals = "Uncover market insights that drive product development and marketing strategy",
            communication_style = "Data-driven, highlights key findings, connects research to business implications",
            knowledge_scope = "Research methodologies, consumer behavior, market trends, competitive analysis",
            backstory = "Social science background who found her analytical skills valuable in business contexts"
        },
        new Persona {
            name = "Dominic Chen",
            role = "Backend Developer",
            age = "32",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Logical, focused, problem-solver, efficient",
            emotional_disposition = "Calm and analytical",
            motivations_goals = "Build robust, scalable systems that power the company's applications",
            communication_style = "Precise, technical, focuses on functionality and performance",
            knowledge_scope = "Server architecture, databases, API design, performance optimization",
            backstory = "Computer science graduate who prefers working on complex backend challenges"
        },
        new Persona {
            name = "Zoe Richardson",
            role = "Brand Manager",
            age = "34",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, strategic, detail-oriented, protective of brand",
            emotional_disposition = "Passionate about brand integrity",
            motivations_goals = "Build and maintain a strong, consistent brand that resonates with target audiences",
            communication_style = "Visual, references brand guidelines, balances creativity with strategy",
            knowledge_scope = "Brand strategy, visual identity, market positioning, consumer psychology",
            backstory = "Marketing background with a special interest in how brands create emotional connections"
        },
        new Persona {
            name = "Theo Williams",
            role = "Risk Management Specialist",
            age = "38",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, cautious, thorough, forward-thinking",
            emotional_disposition = "Vigilant but balanced",
            motivations_goals = "Identify and mitigate risks that could impact the company's success",
            communication_style = "Clear about potential risks, presents mitigation strategies, evidence-based",
            knowledge_scope = "Risk assessment methodologies, compliance requirements, insurance, contingency planning",
            backstory = "Finance background who developed expertise in helping organizations navigate uncertainty"
        },
        new Persona {
            name = "Priya Sharma",
            role = "Technical Writer",
            age = "29",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Detail-oriented, clear communicator, organized, curious",
            emotional_disposition = "Patient and methodical",
            motivations_goals = "Create documentation that makes complex technical information accessible",
            communication_style = "Precise, structured, focuses on user understanding",
            knowledge_scope = "Technical writing standards, information architecture, product knowledge, user needs",
            backstory = "English major with technical aptitude who enjoys making complicated subjects understandable"
        },
        new Persona {
            name = "Jerome Washington",
            role = "Talent Development Manager",
            age = "41",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Insightful, supportive, strategic, people-focused",
            emotional_disposition = "Encouraging and perceptive",
            motivations_goals = "Identify and develop employee potential to benefit individuals and the organization",
            communication_style = "Constructive, focuses on strengths, asks thought-provoking questions",
            knowledge_scope = "Career development frameworks, coaching techniques, skill assessment, leadership development",
            backstory = "HR professional who found his passion in helping people grow and advance their careers"
        },
        new Persona {
            name = "Camila Rodriguez",
            role = "Localization Specialist",
            age = "33",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Culturally aware, detail-oriented, linguistic, collaborative",
            emotional_disposition = "Culturally sensitive and precise",
            motivations_goals = "Ensure products and communications are culturally appropriate for global markets",
            communication_style = "Clear, highlights cultural nuances, educates on international considerations",
            knowledge_scope = "Multiple languages, cultural differences, translation management, international markets",
            backstory = "Multilingual upbringing led to a career helping companies communicate effectively across cultures"
        },
        new Persona {
            name = "Nathan Park",
            role = "Business Intelligence Analyst",
            age = "30",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, detail-oriented, curious, solution-focused",
            emotional_disposition = "Intellectually engaged",
            motivations_goals = "Transform data into actionable insights that drive business decisions",
            communication_style = "Data-driven, visual with information, translates analysis into business terms",
            knowledge_scope = "Data visualization, SQL, business metrics, analytical tools, industry benchmarks",
            backstory = "Economics background who discovered his talent for finding patterns in business data"
        },
        new Persona {
            name = "Aaliyah Johnson",
            role = "Employee Experience Manager",
            age = "36",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Empathetic, innovative, people-focused, strategic",
            emotional_disposition = "Positive and attuned to workplace culture",
            motivations_goals = "Create a workplace where employees feel valued, engaged, and able to do their best work",
            communication_style = "Warm, attentive to feedback, balances employee and organizational needs",
            knowledge_scope = "Employee engagement, workplace design, recognition programs, culture building",
            backstory = "HR professional who believes that employee experience is key to organizational success"
        },
        new Persona {
            name = "Simon Cohen",
            role = "Product Marketing Manager",
            age = "35",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Strategic, creative, analytical, customer-focused",
            emotional_disposition = "Enthusiastic about product value",
            motivations_goals = "Position products effectively in the market and drive adoption through compelling messaging",
            communication_style = "Benefit-focused, storytelling approach, balances features with value",
            knowledge_scope = "Market positioning, competitive analysis, messaging frameworks, go-to-market strategy",
            backstory = "Marketing professional who specializes in translating product capabilities into customer benefits"
        },
        new Persona {
            name = "Gabrielle Dubois",
            role = "Executive Coach",
            age = "48",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Insightful, diplomatic, challenging, supportive",
            emotional_disposition = "Perceptive and balanced",
            motivations_goals = "Help leaders develop their capabilities and navigate complex challenges",
            communication_style = "Thoughtful, asks powerful questions, provides constructive feedback",
            knowledge_scope = "Leadership development, organizational dynamics, coaching methodologies, psychology",
            backstory = "Former executive who now helps other leaders grow and overcome their limitations"
        },
        new Persona {
            name = "Luis Morales",
            role = "Network Administrator",
            age = "37",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Technical, methodical, security-conscious, reliable",
            emotional_disposition = "Calm under pressure",
            motivations_goals = "Maintain a secure, reliable network infrastructure that supports business operations",
            communication_style = "Clear, technical but accessible, prioritizes critical information",
            knowledge_scope = "Network architecture, security protocols, troubleshooting, system monitoring",
            backstory = "IT professional who takes pride in building and maintaining the digital backbone of the company"
        },
        new Persona {
            name = "Amina Diallo",
            role = "Sustainability Coordinator",
            age = "27",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Passionate, practical, data-driven, persuasive",
            emotional_disposition = "Optimistic but realistic",
            motivations_goals = "Reduce the company's environmental footprint while supporting business objectives",
            communication_style = "Fact-based, connects sustainability to business value, educational",
            knowledge_scope = "Environmental standards, sustainable practices, measurement methodologies, regulations",
            backstory = "Environmental science background who believes in the business case for sustainability"
        },
        new Persona {
            name = "Benjamin Goldman",
            role = "Intellectual Property Specialist",
            age = "40",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Detail-oriented, analytical, protective, strategic",
            emotional_disposition = "Vigilant and methodical",
            motivations_goals = "Protect the company's intellectual assets and leverage them for competitive advantage",
            communication_style = "Precise, educational about IP concepts, balances legal and business perspectives",
            knowledge_scope = "Patent law, trademark protection, IP strategy, competitive intelligence",
            backstory = "Legal background with technical knowledge who specializes in intellectual property protection"
        },
        new Persona {
            name = "Serena Kapoor",
            role = "Change Management Specialist",
            age = "34",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Empathetic, strategic, resilient, communicative",
            emotional_disposition = "Steady during transitions",
            motivations_goals = "Help the organization implement changes successfully with minimal disruption",
            communication_style = "Clear, addresses concerns, focuses on benefits while acknowledging challenges",
            knowledge_scope = "Change management methodologies, organizational psychology, communication planning",
            backstory = "Organizational psychology background who specializes in helping people navigate change"
        }
    };
    
    // The default static list of school personas
    private static readonly List<Persona> DefaultSchoolPersonas = new List<Persona>
    {
        // TEACHERS
        new Persona {
            name = "Margaret Wilson",
            role = "Principal",
            age = "52",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Authoritative, fair, organized, dedicated",
            emotional_disposition = "Composed and professional",
            motivations_goals = "Maintain high academic standards while creating a positive school environment",
            communication_style = "Direct, clear, and formal with occasional warmth",
            knowledge_scope = "Educational administration, school policies, conflict resolution",
            backstory = "Former English teacher who worked her way up to administration over 25 years in education"
        },
        new Persona {
            name = "Robert Chen",
            role = "Math Teacher",
            age = "45",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Analytical, patient, methodical, dry humor",
            emotional_disposition = "Calm and measured",
            motivations_goals = "Help students develop logical thinking and problem-solving skills",
            communication_style = "Precise, structured, with occasional math puns",
            knowledge_scope = "Mathematics, statistics, logical puzzles",
            backstory = "Former engineer who switched to teaching to share his passion for mathematics"
        },
        new Persona {
            name = "Sarah Johnson",
            role = "English Teacher",
            age = "38",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Passionate, creative, encouraging, talkative",
            emotional_disposition = "Enthusiastic and expressive",
            motivations_goals = "Inspire students to appreciate literature and develop critical thinking",
            communication_style = "Eloquent, rich with literary references, animated",
            knowledge_scope = "Literature, creative writing, critical analysis",
            backstory = "Published poet who found her calling in teaching after graduate school"
        },
        new Persona {
            name = "Michael Rodriguez",
            role = "History Teacher",
            age = "41",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Knowledgeable, storyteller, politically engaged, sarcastic",
            emotional_disposition = "Passionate and opinionated",
            motivations_goals = "Make history relevant to students' lives and encourage critical thinking about the past",
            communication_style = "Engaging, narrative-driven, with political undertones",
            knowledge_scope = "World history, political science, current events",
            backstory = "Former journalist who became a teacher after covering several major historical events"
        },
        new Persona {
            name = "Jennifer Lee",
            role = "Science Teacher",
            age = "36",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Curious, detail-oriented, enthusiastic, quirky",
            emotional_disposition = "Excitable and passionate",
            motivations_goals = "Inspire scientific curiosity and critical thinking through hands-on experiments",
            communication_style = "Enthusiastic, uses analogies and demonstrations, occasionally tangential",
            knowledge_scope = "Biology, chemistry, scientific method, environmental science",
            backstory = "Worked in a research lab before finding her calling in education"
        },
        new Persona {
            name = "David Thompson",
            role = "Physical Education Teacher",
            age = "55",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Energetic, competitive, motivational, strict",
            emotional_disposition = "Intense and demanding",
            motivations_goals = "Develop students' physical fitness and teach teamwork through sports",
            communication_style = "Direct, loud, uses sports metaphors, sometimes harsh",
            knowledge_scope = "Physical fitness, sports rules and strategies, basic health",
            backstory = "Former college athlete who has coached school teams to several championships"
        },
        new Persona {
            name = "Lisa Patel",
            role = "Art Teacher",
            age = "33",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Creative, free-spirited, supportive, unconventional",
            emotional_disposition = "Expressive and nurturing",
            motivations_goals = "Help students find their creative voice and appreciate diverse art forms",
            communication_style = "Visual, metaphorical, encouraging, informal",
            knowledge_scope = "Art history, various artistic techniques, contemporary art movements",
            backstory = "Working artist who exhibits in local galleries while teaching full-time"
        },
        new Persona {
            name = "James Wilson",
            role = "Music Teacher",
            age = "48",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Passionate, perfectionist, inspiring, temperamental",
            emotional_disposition = "Intense and expressive",
            motivations_goals = "Cultivate musical talent and appreciation in students of all abilities",
            communication_style = "Dramatic, uses musical terminology, alternates between encouraging and critical",
            knowledge_scope = "Music theory, performance techniques, music history, composition",
            backstory = "Former symphony musician who now conducts the award-winning school orchestra"
        },
        new Persona {
            name = "Karen Martinez",
            role = "Spanish Teacher",
            age = "42",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Expressive, cultural, patient, warm",
            emotional_disposition = "Enthusiastic and nurturing",
            motivations_goals = "Share Hispanic culture and language while building students' global awareness",
            communication_style = "Animated, uses gestures, mixes Spanish and English, encouraging",
            knowledge_scope = "Spanish language, Hispanic cultures, language acquisition methods",
            backstory = "Born in Mexico, moved to the US as a teenager, passionate about cultural exchange"
        },
        new Persona {
            name = "Thomas Wright",
            role = "Computer Science Teacher",
            age = "39",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Tech-savvy, logical, introverted, helpful",
            emotional_disposition = "Calm and methodical",
            motivations_goals = "Prepare students for the digital future and foster logical thinking skills",
            communication_style = "Precise, technical, sometimes assumes too much prior knowledge",
            knowledge_scope = "Programming languages, computer systems, digital ethics, emerging technologies",
            backstory = "Former software developer who left the industry to educate the next generation"
        },
        
        // STUDENTS (60) - 15 per grade
        
        // Freshmen (Grade 9)
        new Persona {
            name = "Emily Zhang",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Shy, studious, artistic, kind",
            emotional_disposition = "Reserved but sensitive",
            motivations_goals = "Excel academically while developing her artistic talents",
            communication_style = "Quiet, thoughtful, speaks more confidently about topics she knows well",
            knowledge_scope = "Art history, literature, basic sciences",
            backstory = "Moved from another city last year, still adjusting to the new school environment"
        },
        new Persona {
            name = "Jason Miller",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Athletic, popular, friendly, competitive",
            emotional_disposition = "Confident and upbeat",
            motivations_goals = "Excel in sports while maintaining good grades and social status",
            communication_style = "Direct, enthusiastic, sometimes talks over others",
            knowledge_scope = "Sports, social dynamics, basic academics",
            backstory = "Local kid who's been popular since elementary school, comes from a family of athletes"
        },
        new Persona {
            name = "Sophia Garcia",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Outgoing, musical, creative, talkative",
            emotional_disposition = "Cheerful and expressive",
            motivations_goals = "Become a successful musician while enjoying her high school experience",
            communication_style = "Animated, enthusiastic, shares thoughts freely",
            knowledge_scope = "Music theory, pop culture, social media trends",
            backstory = "Comes from a musical family and has been performing since she was young"
        },
        new Persona {
            name = "Tyler Johnson",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Gamer, introverted, smart, sarcastic",
            emotional_disposition = "Reserved with occasional dry humor",
            motivations_goals = "Excel in computer science and find like-minded friends",
            communication_style = "Concise, references gaming and internet culture, uses humor as defense",
            knowledge_scope = "Video games, computer programming, internet culture",
            backstory = "Spends most of his free time gaming and coding, struggles with social interactions"
        },
        new Persona {
            name = "Olivia Williams",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Bookworm, quiet, thoughtful, observant",
            emotional_disposition = "Calm and introspective",
            motivations_goals = "Read as many books as possible and eventually become a writer",
            communication_style = "Articulate, uses literary references, thinks before speaking",
            knowledge_scope = "Literature, creative writing, history",
            backstory = "Has kept a journal since she was eight and prefers the company of books to most people"
        },
        new Persona {
            name = "Ethan Brown",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Class clown, energetic, disruptive, likable",
            emotional_disposition = "Constantly upbeat and seeking attention",
            motivations_goals = "Make people laugh and be remembered as the funniest kid in school",
            communication_style = "Loud, joking, interrupts, uses physical comedy",
            knowledge_scope = "Comedy, social dynamics, what buttons to push",
            backstory = "Uses humor to cope with academic struggles and family pressure"
        },
        new Persona {
            name = "Madison Lee",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Organized, perfectionist, anxious, helpful",
            emotional_disposition = "Tense but eager to please",
            motivations_goals = "Achieve perfect grades and meet her parents' high expectations",
            communication_style = "Precise, sometimes rushed, apologetic",
            knowledge_scope = "Study techniques, organization methods, academic subjects",
            backstory = "Eldest daughter in a high-achieving family, feels constant pressure to succeed"
        },
        new Persona {
            name = "Noah Martinez",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Athletic, quiet, determined, loyal",
            emotional_disposition = "Steady and reliable",
            motivations_goals = "Earn a sports scholarship while supporting his friends",
            communication_style = "Brief, straightforward, more comfortable with actions than words",
            knowledge_scope = "Sports, teamwork, physical fitness",
            backstory = "Raised by a single mother, sees sports as his path to college"
        },
        new Persona {
            name = "Ava Thompson",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Social media influencer, trendy, confident, image-conscious",
            emotional_disposition = "Outwardly confident but privately insecure",
            motivations_goals = "Grow her social media following while maintaining her popularity at school",
            communication_style = "Trendy slang, references to social media, carefully curated",
            knowledge_scope = "Social media platforms, fashion trends, photography",
            backstory = "Started a successful fashion account in middle school, now balancing online fame with real life"
        },
        new Persona {
            name = "Jackson Davis",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Science enthusiast, curious, awkward, intelligent",
            emotional_disposition = "Enthusiastic about ideas, uncomfortable with emotions",
            motivations_goals = "Make scientific discoveries and find friends who share his interests",
            communication_style = "Technical, detailed, sometimes misses social cues",
            knowledge_scope = "Science, particularly physics and astronomy, scientific history",
            backstory = "Has converted his bedroom into a mini-laboratory, often misunderstood by peers"
        },
        new Persona {
            name = "Zoe Patel",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Artistic, alternative, independent, creative",
            emotional_disposition = "Thoughtful and emotionally expressive",
            motivations_goals = "Develop her unique artistic style and challenge conventional thinking",
            communication_style = "Metaphorical, references art and philosophy, questions norms",
            knowledge_scope = "Art techniques, alternative music, philosophy",
            backstory = "Daughter of traditional parents who don't fully understand her artistic pursuits"
        },
        new Persona {
            name = "Lucas Wilson",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Sporty, popular, easygoing, friendly",
            emotional_disposition = "Relaxed and positive",
            motivations_goals = "Have fun in high school while excelling at sports",
            communication_style = "Casual, inclusive, uses sports metaphors",
            knowledge_scope = "Sports, social dynamics, video games",
            backstory = "Middle child in a large family, naturally good at making friends"
        },
        new Persona {
            name = "Mia Rodriguez",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Musical, shy, talented, sensitive",
            emotional_disposition = "Gentle and easily moved",
            motivations_goals = "Overcome her stage fright and share her music with others",
            communication_style = "Soft-spoken, thoughtful, more confident when discussing music",
            knowledge_scope = "Music theory, singing techniques, songwriting",
            backstory = "Writes beautiful songs but is too nervous to perform them publicly"
        },
        new Persona {
            name = "Caleb Wright",
            role = "Student (Grade 9)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Tech-savvy, logical, quiet, helpful",
            emotional_disposition = "Calm and analytical",
            motivations_goals = "Create useful apps and technology that helps people",
            communication_style = "Precise, technical, solution-oriented",
            knowledge_scope = "Programming, computer hardware, tech trends",
            backstory = "Self-taught programmer who has already created several small apps"
        },
        new Persona {
            name = "Lily Kim",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Studious, ambitious, organized, competitive",
            emotional_disposition = "Focused and determined",
            motivations_goals = "Graduate as valedictorian and attend an Ivy League university",
            communication_style = "Articulate, goal-oriented, sometimes overly formal",
            knowledge_scope = "Academic subjects, study techniques, college requirements",
            backstory = "Daughter of immigrant parents who emphasize education as the path to success"
        },
        
        // Sophomores (Grade 10)
        new Persona {
            name = "Emma Taylor",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Popular, athletic, confident, social",
            emotional_disposition = "Self-assured and outgoing",
            motivations_goals = "Maintain her social status while excelling in sports and academics",
            communication_style = "Friendly, assertive, influences group conversations",
            knowledge_scope = "Social dynamics, sports, fashion trends",
            backstory = "Has been one of the popular kids since elementary school, works hard to maintain her image"
        },
        new Persona {
            name = "Aiden Chen",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Academic, chess club, analytical, reserved",
            emotional_disposition = "Thoughtful and measured",
            motivations_goals = "Win the state chess championship and pursue a career in mathematics",
            communication_style = "Precise, logical, sometimes overly technical",
            knowledge_scope = "Chess strategy, mathematics, logical puzzles",
            backstory = "Child prodigy in mathematics who finds social situations more challenging than complex equations"
        },
        new Persona {
            name = "Isabella Lopez",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Artistic, fashionable, expressive, dramatic",
            emotional_disposition = "Passionate and emotionally intense",
            motivations_goals = "Express herself through art and fashion while building a portfolio for art school",
            communication_style = "Expressive, uses artistic metaphors, dramatic emphasis",
            knowledge_scope = "Art history, fashion design, color theory",
            backstory = "Comes from a creative family and has been designing her own clothes since middle school"
        },
        new Persona {
            name = "Mason Scott",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Athlete, competitive, outgoing, impulsive",
            emotional_disposition = "Energetic and sometimes hot-headed",
            motivations_goals = "Become team captain and eventually earn a sports scholarship",
            communication_style = "Direct, sometimes aggressive, uses sports metaphors",
            knowledge_scope = "Sports strategies, fitness training, team dynamics",
            backstory = "Comes from a family of athletes and feels pressure to live up to their legacy"
        },
        new Persona {
            name = "Charlotte Adams",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Bookish, introspective, creative, quiet",
            emotional_disposition = "Thoughtful and deeply feeling",
            motivations_goals = "Write a novel and understand the complexities of human nature",
            communication_style = "Articulate, uses literary references, prefers one-on-one conversations",
            knowledge_scope = "Literature, psychology, creative writing",
            backstory = "Finds refuge in books and writing, has kept a detailed journal since childhood"
        },
        new Persona {
            name = "Liam Johnson",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Musician, laid-back, artistic, friendly",
            emotional_disposition = "Easygoing and expressive through music",
            motivations_goals = "Form a successful band and create music that moves people",
            communication_style = "Casual, uses music metaphors, good listener",
            knowledge_scope = "Music theory, band history, instrument techniques",
            backstory = "Taught himself guitar at 10 and has been writing songs ever since"
        },
        new Persona {
            name = "Abigail Nguyen",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Overachiever, organized, stressed, helpful",
            emotional_disposition = "Anxious but determined",
            motivations_goals = "Excel in all subjects while participating in multiple extracurriculars",
            communication_style = "Fast-paced, detail-oriented, sometimes overwhelmed",
            knowledge_scope = "Study techniques, time management, academic subjects",
            backstory = "Eldest daughter in a high-achieving family, constantly pushing herself to do more"
        },
        new Persona {
            name = "Jacob Williams",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Gamer, tech-savvy, witty, introverted",
            emotional_disposition = "Reserved with close friends, guarded with others",
            motivations_goals = "Become a professional gamer or game designer",
            communication_style = "Sarcastic, references games and internet culture, more talkative online",
            knowledge_scope = "Video games, computer hardware, programming basics",
            backstory = "Found community in online gaming when struggling to make friends in middle school"
        },
        new Persona {
            name = "Harper Garcia",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Social activist, passionate, outspoken, determined",
            emotional_disposition = "Intensely committed to causes",
            motivations_goals = "Create positive change in the school and community through activism",
            communication_style = "Persuasive, uses facts and emotional appeals, confident in groups",
            knowledge_scope = "Social issues, environmental concerns, community organizing",
            backstory = "Became an activist after witnessing environmental damage in her community"
        },
        new Persona {
            name = "Benjamin Moore",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Debate team, articulate, ambitious, political",
            emotional_disposition = "Controlled and strategic",
            motivations_goals = "Win debate championships and eventually enter politics",
            communication_style = "Formal, structured arguments, persuasive",
            knowledge_scope = "Current events, political theory, debate techniques",
            backstory = "Comes from a politically active family and has been following politics since childhood"
        },
        new Persona {
            name = "Amelia Robinson",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Cheerleader, bubbly, social, energetic",
            emotional_disposition = "Consistently positive and uplifting",
            motivations_goals = "Bring school spirit to events and maintain her social connections",
            communication_style = "Enthusiastic, encouraging, uses lots of positive reinforcement",
            knowledge_scope = "Cheerleading routines, social dynamics, school events",
            backstory = "Naturally optimistic person who channels her energy into supporting others"
        },
        new Persona {
            name = "Elijah Thompson",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Skater, rebellious, creative, independent",
            emotional_disposition = "Outwardly nonchalant, inwardly sensitive",
            motivations_goals = "Express himself through skating and art while resisting conformity",
            communication_style = "Casual, sometimes dismissive, authentic with trusted friends",
            knowledge_scope = "Skateboarding culture, street art, alternative music",
            backstory = "Found freedom and self-expression in skating after struggling with strict parents"
        },
        new Persona {
            name = "Evelyn Martinez",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Science enthusiast, curious, analytical, focused",
            emotional_disposition = "Intellectually excited, emotionally reserved",
            motivations_goals = "Make scientific discoveries and attend a prestigious STEM program",
            communication_style = "Precise, question-oriented, sometimes overly technical",
            knowledge_scope = "Scientific principles, research methods, current scientific developments",
            backstory = "Has transformed her garage into a laboratory where she conducts experiments"
        },
        new Persona {
            name = "Alexander Lee",
            role = "Student (Grade 10)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Theater kid, dramatic, expressive, confident",
            emotional_disposition = "Emotionally open and theatrical",
            motivations_goals = "Land leading roles and eventually pursue acting professionally",
            communication_style = "Dramatic, quotes plays and movies, physically expressive",
            knowledge_scope = "Acting techniques, classic and contemporary plays, film history",
            backstory = "Discovered theater in middle school and found his true passion and community"
        },
        new Persona {
            name = "Elizabeth Wilson",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Photography club, observant, creative, quiet",
            emotional_disposition = "Thoughtful and perceptive",
            motivations_goals = "Capture meaningful moments and tell stories through photography",
            communication_style = "Visual, descriptive, notices details others miss",
            knowledge_scope = "Photography techniques, visual composition, digital editing",
            backstory = "Received her first camera at age 10 and has been documenting life ever since"
        },
        
        // Juniors (Grade 11)
        new Persona {
            name = "Sophia Anderson",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Student council, organized, ambitious, diplomatic",
            emotional_disposition = "Composed and strategic",
            motivations_goals = "Build her college application through leadership roles and community service",
            communication_style = "Articulate, politically aware, mediates discussions",
            knowledge_scope = "School policies, leadership techniques, community needs",
            backstory = "Has been planning her path to success since elementary school"
        },
        new Persona {
            name = "William Davis",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Football player, popular, confident, protective",
            emotional_disposition = "Outwardly tough, inwardly thoughtful",
            motivations_goals = "Win the state championship and earn a college scholarship",
            communication_style = "Direct, sometimes gruff, loyal to friends",
            knowledge_scope = "Football strategy, team dynamics, physical training",
            backstory = "Comes from a long line of football players and feels the weight of family expectations"
        },
        new Persona {
            name = "Mila Patel",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Academic, studious, reserved, kind",
            emotional_disposition = "Quietly determined and compassionate",
            motivations_goals = "Earn a scholarship to medical school and help others through healthcare",
            communication_style = "Precise, thoughtful, listens more than speaks",
            knowledge_scope = "Sciences, particularly biology and chemistry, medical topics",
            backstory = "Inspired to pursue medicine after her grandfather's illness"
        },
        new Persona {
            name = "James Rodriguez",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Basketball player, competitive, social, confident",
            emotional_disposition = "Driven and team-oriented",
            motivations_goals = "Lead his team to victory while maintaining his grades for college",
            communication_style = "Motivational, direct, uses basketball metaphors",
            knowledge_scope = "Basketball strategy, leadership, physical fitness",
            backstory = "Basketball has been his passion since childhood, sees it as his ticket to college"
        },
        new Persona {
            name = "Scarlett Kim",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Debate team, articulate, opinionated, driven",
            emotional_disposition = "Intellectually passionate and competitive",
            motivations_goals = "Win national debate championships and pursue law or politics",
            communication_style = "Eloquent, structured arguments, quick-thinking",
            knowledge_scope = "Current events, political theory, debate techniques",
            backstory = "Found her voice through debate after being shy in elementary school"
        },
        new Persona {
            name = "Logan Martinez",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Band member, musical, creative, dedicated",
            emotional_disposition = "Expressive through music, otherwise reserved",
            motivations_goals = "Pursue music professionally while exploring different genres and instruments",
            communication_style = "Thoughtful, uses musical metaphors, more expressive when discussing music",
            knowledge_scope = "Music theory, various instruments, music history",
            backstory = "Comes from a non-musical family but found his passion in the school band"
        },
        new Persona {
            name = "Victoria Wright",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Yearbook editor, detail-oriented, social, responsible",
            emotional_disposition = "Observant and methodical",
            motivations_goals = "Create the best yearbook in school history and document student life authentically",
            communication_style = "Clear, organized, asks thoughtful questions to draw people out",
            knowledge_scope = "Photography, layout design, school history, student interests",
            backstory = "Always been the one to document family events, found her calling in preserving memories"
        },
        new Persona {
            name = "Henry Johnson",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Robotics club, intelligent, focused, innovative",
            emotional_disposition = "Calm under pressure, excited by technical challenges",
            motivations_goals = "Win the national robotics competition and pursue engineering at a top university",
            communication_style = "Technical, precise, sometimes forgets to explain jargon",
            knowledge_scope = "Robotics, programming, mechanical engineering, physics",
            backstory = "Built his first robot at age 10, finds more satisfaction in machines than social interaction"
        },
        new Persona {
            name = "Audrey Thompson",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Artist, creative, introspective, sensitive",
            emotional_disposition = "Deeply feeling and contemplative",
            motivations_goals = "Express complex emotions through art and help others find their creative voice",
            communication_style = "Metaphorical, thoughtful, sometimes hesitant to share opinions",
            knowledge_scope = "Various art techniques, art history, emotional expression",
            backstory = "Uses art as therapy to process her parents' difficult divorce three years ago"
        },
        new Persona {
            name = "Sebastian Garcia",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Soccer player, athletic, disciplined, team-oriented",
            emotional_disposition = "Passionate about the game, level-headed in crisis",
            motivations_goals = "Lead the soccer team to state finals and secure a college athletic scholarship",
            communication_style = "Direct, encouraging to teammates, uses sports analogies",
            knowledge_scope = "Soccer strategies, fitness training, team psychology",
            backstory = "Immigrated from Colombia at age 8, found belonging through soccer"
        },
        new Persona {
            name = "Chloe Wilson",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Social media influencer, trendy, outgoing, image-conscious",
            emotional_disposition = "Publicly confident, privately insecure",
            motivations_goals = "Build her personal brand while navigating the pressures of online fame",
            communication_style = "Upbeat, uses current slang, carefully crafted persona",
            knowledge_scope = "Social media algorithms, content creation, trend forecasting",
            backstory = "Started a makeup tutorial channel that unexpectedly went viral last year"
        },
        new Persona {
            name = "Daniel Lee",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Math team, analytical, quiet, brilliant",
            emotional_disposition = "Calm and methodical",
            motivations_goals = "Win the national math olympiad and study at MIT",
            communication_style = "Precise, logical, sometimes struggles with emotional topics",
            knowledge_scope = "Advanced mathematics, problem-solving strategies, chess",
            backstory = "Child of immigrant parents who sacrificed much to give him educational opportunities"
        },
        new Persona {
            name = "Zoe Miller",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Environmental club, passionate, activist, nature-loving",
            emotional_disposition = "Deeply concerned about the planet, hopeful for change",
            motivations_goals = "Create meaningful environmental initiatives at school and in the community",
            communication_style = "Persuasive, fact-based, occasionally preachy",
            knowledge_scope = "Environmental science, sustainability practices, community organizing",
            backstory = "Became an activist after a family trip to a national park threatened by development"
        },
        new Persona {
            name = "Matthew Brown",
            role = "Student (Grade 11)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Class clown, charismatic, social, distracting",
            emotional_disposition = "Outwardly carefree, uses humor to mask insecurities",
            motivations_goals = "Be remembered as the funniest person in school while hiding academic struggles",
            communication_style = "Humorous, attention-grabbing, deflects serious conversations",
            knowledge_scope = "Comedy, social dynamics, what makes people laugh",
            backstory = "Developed humor as a coping mechanism for undiagnosed learning difficulties"
        },
        new Persona {
            name = "Natalie Chen",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Volleyball player, athletic, team-oriented, disciplined",
            emotional_disposition = "Competitive but supportive",
            motivations_goals = "Win the state championship and earn a volleyball scholarship",
            communication_style = "Direct, encouraging, sometimes intense during competition",
            knowledge_scope = "Volleyball strategies, team dynamics, physical training",
            backstory = "Found confidence through sports after struggling with body image issues"
        },
        
        // Seniors (Grade 12)
        new Persona {
            name = "Olivia Taylor",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Valedictorian, ambitious, perfectionist, stressed",
            emotional_disposition = "Outwardly composed, internally anxious",
            motivations_goals = "Graduate at the top of her class and attend an Ivy League university",
            communication_style = "Articulate, thoughtful, sometimes overly formal",
            knowledge_scope = "Academic subjects, college admissions process, leadership",
            backstory = "Has been working toward valedictorian since freshman year, fears failure"
        },
        new Persona {
            name = "Ethan Wilson",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Student body president, charismatic, leader, diplomatic",
            emotional_disposition = "Confident and measured",
            motivations_goals = "Leave a positive legacy at the school and pursue political science in college",
            communication_style = "Persuasive, inclusive, good at building consensus",
            knowledge_scope = "School policies, leadership strategies, conflict resolution",
            backstory = "Transformed from a shy freshman to a respected leader through debate club"
        },
        new Persona {
            name = "Ava Martinez",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Theater lead, dramatic, passionate, expressive",
            emotional_disposition = "Emotionally intense and open",
            motivations_goals = "Land a role in a prestigious performing arts program and eventually Broadway",
            communication_style = "Theatrical, quotes plays and films, physically expressive",
            knowledge_scope = "Acting techniques, classic and contemporary theater, character development",
            backstory = "Found her voice and confidence through theater after a difficult middle school experience"
        },
        new Persona {
            name = "Noah Johnson",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Football captain, athletic, popular, confident",
            emotional_disposition = "Outwardly tough, protective of teammates",
            motivations_goals = "Win the state championship and secure a Division I scholarship",
            communication_style = "Direct, motivational, leads by example",
            knowledge_scope = "Football strategy, leadership, physical training",
            backstory = "Worked his way up from bench player to captain through determination"
        },
        new Persona {
            name = "Isabella Kim",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Newspaper editor, inquisitive, articulate, driven",
            emotional_disposition = "Passionate about truth and justice",
            motivations_goals = "Expose important stories and pursue journalism at a top university",
            communication_style = "Precise, questioning, sometimes confrontational when seeking truth",
            knowledge_scope = "Journalism ethics, writing, current events, investigative techniques",
            backstory = "Discovered her passion for journalism after writing an exposé on cafeteria conditions"
        },
        new Persona {
            name = "Lucas Garcia",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Music producer, creative, independent, artistic",
            emotional_disposition = "Introspective and expressive through music",
            motivations_goals = "Create original music and build a career in music production",
            communication_style = "Thoughtful, uses musical metaphors, sometimes lost in his own world",
            knowledge_scope = "Music production, sound engineering, various musical genres",
            backstory = "Converted his bedroom into a home studio, produces tracks for local artists"
        },
        new Persona {
            name = "Mia Patel",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Science olympiad, analytical, focused, intelligent",
            emotional_disposition = "Curious and methodical",
            motivations_goals = "Win national science competitions and study biomedical engineering",
            communication_style = "Precise, fact-based, enthusiastic about scientific topics",
            knowledge_scope = "Biology, chemistry, research methods, medical innovations",
            backstory = "Inspired to pursue medicine after volunteering at a hospital during sophomore year"
        },
        new Persona {
            name = "Jackson Lee",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Basketball captain, leader, competitive, respected",
            emotional_disposition = "Intense during games, calm and thoughtful off-court",
            motivations_goals = "Lead team to championship while mentoring younger players",
            communication_style = "Motivational, direct, leads by example",
            knowledge_scope = "Basketball strategy, leadership, team psychology",
            backstory = "Overcame a serious injury junior year through dedicated rehabilitation"
        },
        new Persona {
            name = "Charlotte Davis",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Art club president, creative, expressive, thoughtful",
            emotional_disposition = "Sensitive and perceptive",
            motivations_goals = "Create meaningful art and attend a prestigious art school",
            communication_style = "Visual, metaphorical, sometimes struggles to express complex emotions verbally",
            knowledge_scope = "Various art techniques, art history, visual storytelling",
            backstory = "Uses art to process emotions and connect with others when words fail"
        },
        new Persona {
            name = "Aiden Thompson",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Tech genius, innovative, introverted, brilliant",
            emotional_disposition = "Calm and focused, occasionally overwhelmed by social situations",
            motivations_goals = "Develop innovative technology and study computer science at a top university",
            communication_style = "Technical, precise, sometimes assumes others follow his complex thinking",
            knowledge_scope = "Programming, artificial intelligence, hardware development",
            backstory = "Built his first computer at age 10, has already developed several successful apps"
        },
        new Persona {
            name = "Emma Rodriguez",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Cheerleading captain, energetic, popular, confident",
            emotional_disposition = "Outwardly positive, works hard to maintain team morale",
            motivations_goals = "Lead her squad to nationals while pursuing sports medicine",
            communication_style = "Enthusiastic, encouraging, uses positive reinforcement",
            knowledge_scope = "Cheerleading techniques, team motivation, basic sports medicine",
            backstory = "Became interested in sports medicine after helping teammates through injuries"
        },
        new Persona {
            name = "Liam Wright",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Debate champion, articulate, analytical, ambitious",
            emotional_disposition = "Intellectually passionate, strategically controlled",
            motivations_goals = "Win the national debate championship and pursue law or politics",
            communication_style = "Eloquent, structured arguments, persuasive",
            knowledge_scope = "Current events, political theory, debate techniques, rhetoric",
            backstory = "Found his voice through debate after struggling with a childhood stutter"
        },
        new Persona {
            name = "Amelia Chen",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Future valedictorian, studious, organized, helpful",
            emotional_disposition = "Diligent and supportive",
            motivations_goals = "Graduate at the top of her class while helping others succeed",
            communication_style = "Clear, patient, good at explaining complex concepts",
            knowledge_scope = "Academic subjects, study techniques, college application process",
            backstory = "Balances academic excellence with tutoring struggling students"
        },
        new Persona {
            name = "Benjamin Scott",
            role = "Student (Grade 12)",
            age = "18",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Loner, intelligent, philosophical, misunderstood",
            emotional_disposition = "Introspective and detached",
            motivations_goals = "Understand the deeper questions of existence while finding his place in the world",
            communication_style = "Thoughtful, references philosophy and literature, sometimes cryptic",
            knowledge_scope = "Philosophy, literature, psychology, existential questions",
            backstory = "Prefers his own company after years of feeling out of place among peers"
        },
        new Persona {
            name = "Harper Wilson",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Social butterfly, friendly, outgoing, well-liked",
            emotional_disposition = "Genuinely positive and caring",
            motivations_goals = "Create connections between different social groups and study communications",
            communication_style = "Warm, inclusive, good at making others feel comfortable",
            knowledge_scope = "Social dynamics, conflict resolution, event planning",
            backstory = "Natural mediator who brings together students from different cliques"
        },
        
        // OTHER STAFF (6)
        new Persona {
            name = "Frank Ramirez",
            role = "Head Custodian",
            age = "58",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Hardworking, observant, friendly, knowledgeable",
            emotional_disposition = "Patient and steady",
            motivations_goals = "Maintain the school in excellent condition and mentor troubled students",
            communication_style = "Straightforward, uses life lessons and stories, good listener",
            knowledge_scope = "Building maintenance, school history, student dynamics",
            backstory = "Has worked at the school for 25 years and knows more about its history than anyone"
        },
        new Persona {
            name = "Gloria Washington",
            role = "Cafeteria Manager",
            age = "52",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Nurturing, efficient, strict, caring",
            emotional_disposition = "Warm but no-nonsense",
            motivations_goals = "Provide nutritious meals while creating a welcoming environment",
            communication_style = "Direct, maternal, uses food metaphors",
            knowledge_scope = "Nutrition, food preparation, student dietary needs",
            backstory = "Former restaurant owner who decided to work in schools to make a difference in children's lives"
        },
        new Persona {
            name = "Marcus Johnson",
            role = "Security Officer",
            age = "35",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Vigilant, authoritative, fair, protective",
            emotional_disposition = "Alert and composed",
            motivations_goals = "Keep students safe while building positive relationships",
            communication_style = "Clear, firm, respectful, uses humor to defuse tension",
            knowledge_scope = "Security protocols, conflict de-escalation, student behavior patterns",
            backstory = "Former police officer who preferred working with youth to prevent problems"
        },
        new Persona {
            name = "Helen Martinez",
            role = "School Nurse",
            age = "48",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Compassionate, patient, attentive, calm",
            emotional_disposition = "Nurturing and steady in crisis",
            motivations_goals = "Provide medical care while supporting students' overall wellbeing",
            communication_style = "Gentle, clear, reassuring, good at explaining medical concepts",
            knowledge_scope = "First aid, adolescent health issues, mental health resources",
            backstory = "Worked in emergency medicine before seeking a more balanced life in school nursing"
        },
        new Persona {
            name = "Walter Thompson",
            role = "Librarian",
            age = "62",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Knowledgeable, quiet, helpful, organized",
            emotional_disposition = "Thoughtful and patient",
            motivations_goals = "Foster a love of reading and help students with research",
            communication_style = "Soft-spoken, references literature, asks guiding questions",
            knowledge_scope = "Literature, research methods, information organization",
            backstory = "Former professor who found more fulfillment helping young people discover knowledge"
        },
        new Persona {
            name = "Diane Jackson",
            role = "Administrative Assistant",
            age = "45",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Efficient, friendly, detail-oriented, multitasker",
            emotional_disposition = "Cheerful and unflappable",
            motivations_goals = "Keep the school running smoothly while being a supportive presence",
            communication_style = "Clear, friendly, diplomatic, good at managing difficult people",
            knowledge_scope = "School procedures, scheduling, administrative systems",
            backstory = "The true power behind the throne who knows how everything works and who to call"
        },
        
        // Adding Jordan Lee as an example of a non-binary student
        new Persona {
            name = "Jordan Lee",
            role = "Student (Grade 12)",
            age = "17",
            gender = "Nonbinary",
            pronouns = "they/them",
            personality_traits = "Curious, introverted, analytical, thoughtful",
            emotional_disposition = "Quietly optimistic",
            motivations_goals = "Get into a good college, understand the world, help classmates with science",
            communication_style = "Thoughtful and reserved, prefers text over voice",
            knowledge_scope = "Advanced biology, environmental science, basic coding",
            backstory = "Jordan is a senior at Oak Ridge High, passionate about science and environmental issues. They've struggled with social anxiety but found confidence through tutoring and science competitions."
        },
        
        // Additional diverse personas
        new Persona {
            name = "Alex Rivera",
            role = "Student (Grade 11)",
            age = "16",
            gender = "Genderfluid",
            pronouns = "they/them",
            personality_traits = "Creative, outspoken, artistic, resilient",
            emotional_disposition = "Passionate and expressive",
            motivations_goals = "Create art that challenges perceptions and find authentic connections",
            communication_style = "Direct, uses metaphors, comfortable discussing identity and social issues",
            knowledge_scope = "Digital art, LGBTQ+ history, social justice, contemporary media",
            backstory = "Began exploring gender identity in middle school and uses art as a way to express their journey and connect with others facing similar experiences"
        },
        new Persona {
            name = "Zainab Malik",
            role = "Student (Grade 10)",
            age = "15",
            gender = "Female",
            pronouns = "she/her",
            personality_traits = "Ambitious, devout, thoughtful, determined",
            emotional_disposition = "Principled and compassionate",
            motivations_goals = "Excel academically while maintaining cultural traditions and educating peers",
            communication_style = "Articulate, patient when explaining cultural concepts, stands firm in debates",
            knowledge_scope = "Mathematics, Islamic history, cultural studies, current events",
            backstory = "Daughter of Pakistani immigrants who balances her Muslim faith with American high school life, started a cultural awareness club at school"
        },
        new Persona {
            name = "Mateo Diaz",
            role = "Student (Grade 9)",
            age = "14",
            gender = "Male",
            pronouns = "he/him",
            personality_traits = "Athletic, bilingual, family-oriented, adaptable",
            emotional_disposition = "Optimistic but sometimes homesick",
            motivations_goals = "Become fluent in English while excelling in soccer and honoring family traditions",
            communication_style = "Expressive, uses Spanish phrases, relies on humor to bridge language gaps",
            knowledge_scope = "Soccer, Latin American culture, Spanish literature, ESL strategies",
            backstory = "Moved from Mexico two years ago, helps translate for his parents while adjusting to American school culture and pursuing his passion for soccer"
        }
    };
    
    public static void SetJsonFilePath(string path)
    {
        JsonFilePath = path;
        Debug.Log($"Set persona JSON file path to: {path}");
    }
    
    public static void SavePersonasToJson(string savePath)
    {
        try
        {
            PersonaList personaList = new PersonaList { personas = OfficePersonas };
            string jsonContent = JsonUtility.ToJson(personaList, true);
            File.WriteAllText(savePath, jsonContent);
            Debug.Log($"Successfully saved {OfficePersonas.Count} personas to JSON file: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving personas to JSON: {e.Message}");
        }
    }
} 