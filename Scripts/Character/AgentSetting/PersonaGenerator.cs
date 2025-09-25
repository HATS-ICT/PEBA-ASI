using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class PersonaGenerator
{
    private static readonly string[] MaleNames = {
        // Western names
        "James", "John", "Michael", "William", "David", "Richard", "Thomas", "Mark", "Charles", "Steven",
        "Robert", "Joseph", "Daniel", "Matthew", "Anthony", "Donald", "Paul", "Andrew", "Joshua", "Kenneth",
        "Kevin", "Brian", "George", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob",
        "Gary", "Nicholas", "Eric", "Jonathan", "Stephen", "Larry", "Justin", "Scott", "Brandon", "Benjamin",
        "Samuel", "Gregory", "Alexander", "Patrick", "Frank", "Raymond", "Jack", "Dennis", "Jerry", "Tyler",
        "Aaron", "Adam", "Adrian", "Alan", "Albert", "Alfred", "Allen", "Arthur", "Austin", "Barry",
        "Blake", "Bradley", "Brett", "Bruce", "Bryan", "Calvin", "Cameron", "Carl", "Chad", "Christian",
        "Christopher", "Clarence", "Claude", "Clayton", "Clifford", "Clinton", "Corey", "Craig", "Curtis", "Dale",
        "Darrell", "Dean", "Derek", "Derrick", "Douglas", "Dustin", "Dylan", "Earl", "Eugene", "Francis",
        "Frederick", "Gabriel", "Gerald", "Gilbert", "Glenn", "Gordon", "Graham", "Grant", "Harold", "Harry",
        // Hispanic names
        "Carlos", "Juan", "Miguel", "Luis", "Jose", "Antonio", "Francisco", "Pedro", "Manuel", "Ricardo",
        "Diego", "Rafael", "Ramon", "Fernando", "Alejandro", "Roberto", "Eduardo", "Javier", "Victor", "Mario",
        "Enrique", "Alberto", "Raul", "Marco", "Alfonso", "Salvador", "Oscar", "Guillermo", "Felix", "Sergio",
        // Asian names
        "Wei", "Ming", "Yong", "Jun", "Feng", "Lei", "Gang", "Jian", "Xiang", "Hui",
        "Hao", "Yi", "Bin", "Cheng", "Tao", "Kai", "Song", "Kuan", "Jie", "Ping",
        "Jin", "Hiroshi", "Takashi", "Kenji", "Akira", "Daisuke", "Ryu", "Kazuki", "Yuki", "Satoshi",
        
        // Middle Eastern names
        "Ali", "Mohammed", "Ahmad", "Hassan", "Hussein", "Omar", "Khalid", "Tariq", "Yusuf", "Ibrahim",
        "Karim", "Mustafa", "Samir", "Jamal", "Rashid", "Malik", "Amir", "Hamza", "Zaid", "Bilal",
        // African names
        "Kwame", "Kofi", "Kwesi", "Kojo", "Kweku", "Abebe", "Chibueze", "Oluwaseun", "Babajide", "Oluwafemi",
        "Chidi", "Folami", "Tendai", "Themba", "Mandla", "Thabo", "Sipho", "Blessing", "Chijioke", "Olayinka",
        // Indian names
        "Raj", "Amit", "Arun", "Vijay", "Rahul", "Sunil", "Rajesh", "Sanjay", "Vikram", "Ajay",
        "Anil", "Deepak", "Prakash", "Ramesh", "Suresh", "Vinod", "Rakesh", "Ashok", "Dinesh", "Mahesh"
    };

    private static readonly string[] FemaleNames = {
        // Western names
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen",
        "Nancy", "Lisa", "Betty", "Margaret", "Sandra", "Ashley", "Dorothy", "Kimberly", "Emily", "Donna",
        "Michelle", "Carol", "Amanda", "Melissa", "Deborah", "Stephanie", "Rebecca", "Laura", "Sharon", "Cynthia",
        "Kathleen", "Amy", "Angela", "Shirley", "Anna", "Ruth", "Brenda", "Pamela", "Nicole", "Katherine",
        "Samantha", "Christine", "Emma", "Catherine", "Debra", "Rachel", "Carolyn", "Janet", "Virginia", "Maria",
        "Alice", "Andrea", "Ann", "Beverly", "Bonnie", "Caroline", "Cassandra", "Charlotte", "Christina", "Claire",
        "Danielle", "Diana", "Eleanor", "Ellen", "Evelyn", "Felicia", "Frances", "Georgia", "Grace", "Heather",
        "Helen", "Irene", "Jackie", "Jane", "Julia", "Kelly", "Lauren", "Lillian", "Lucy", "Madeline",
        "Martha", "Megan", "Melanie", "Monica", "Natalie", "Olivia", "Paige", "Paula", "Rose", "Ruby",
        "Sophie", "Tiffany", "Victoria", "Wendy", "Yvonne", "Zoe", "Abigail", "Allison", "April", "Audrey",
        // Hispanic names
        "Ana", "Sofia", "Isabella", "Carmen", "Rosa", "Elena", "Lucia", "Adriana", "Gabriela", "Valentina",
        "Mariana", "Victoria", "Camila", "Daniela", "Catalina", "Isabel", "Valeria", "Alejandra", "Monica", "Claudia",
        "Beatriz", "Teresa", "Raquel", "Silvia", "Mercedes", "Pilar", "Dolores", "Cristina", "Lorena", "Marisol",
        // Asian names
        "Mei", "Xia", "Ling", "Yan", "Qing", "Hui", "Xiu", "Ying", "Jing", "Hong",
        "Yuki", "Sakura", "Haruka", "Yui", "Aoi", "Kaori", "Akiko", "Yoko", "Naomi", "Keiko",
        
        // Middle Eastern names
        "Fatima", "Aisha", "Amira", "Layla", "Zainab", "Noor", "Rania", "Yasmin", "Leila", "Maryam",
        "Samira", "Hana", "Nadia", "Rana", "Dalia", "Zahra", "Salma", "Farida", "Malak", "Reem",
        // African names
        "Amara", "Chioma", "Adanna", "Zainab", "Aisha", "Fatima", "Aminata", "Abena", "Efua", "Afia",
        "Thandiwe", "Zuri", "Amina", "Aaliyah", "Safiya", "Folami", "Makena", "Zalika", "Eshe", "Imani",
        // Indian names
        "Priya", "Neha", "Anjali", "Pooja", "Divya", "Sneha", "Swati", "Kavita", "Deepika", "Anita",
        "Meera", "Shanti", "Lakshmi", "Kiran", "Asha", "Sunita", "Rekha", "Jyoti", "Usha", "Geeta"
    };

    private static readonly string[] Surnames = {
        // Western surnames
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Wilson", "Anderson", "Taylor", "Thomas", "Moore", "Jackson", "Martin", "Lee",
        "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker",
        "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green",
        "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts", "Gomez",
        "Phillips", "Evans", "Turner", "Diaz", "Parker", "Cruz", "Edwards", "Collins", "Reyes", "Stewart",
        "Morris", "Murphy", "Rogers", "Cook", "Morgan", "Peterson", "Cooper", "Reed", "Bailey", "Bell",
        "Bennett", "Wood", "Brooks", "Kelly", "Howard", "Ward", "Cox", "Richardson", "Watson", "Morgan",
        // Hispanic surnames
        "Garcia", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Perez", "Sanchez", "Ramirez", "Torres",
        "Flores", "Rivera", "Morales", "Ortiz", "Chavez", "Medina", "Castro", "Silva", "Mendoza", "Vargas",
        // Asian surnames
        "Wang", "Li", "Zhang", "Liu", "Chen", "Yang", "Huang", "Zhao", "Wu", "Zhou",
        "Tanaka", "Suzuki", "Sato", "Watanabe", "Takahashi", "Yamamoto", "Nakamura", "Kobayashi", "Kato", "Ito",
        "Kim", "Lee", "Park", "Choi", "Jung", "Kang", "Cho", "Chang", "Yoon", "Han",
        // Middle Eastern surnames
        "Khan", "Ahmed", "Hassan", "Ali", "Ibrahim", "Rahman", "Sheikh", "Malik", "Qureshi",
        "Mahmoud", "Hussain", "Mirza", "Aziz", "Khalil", "Abbas", "Saleh", "Raza", "Hamid", "Farooq",
        // African surnames
        "Mensah", "Okafor", "Mwangi", "Adebayo", "Okonkwo", "Mutua", "Ndlovu", "Dube", "Moyo", "Nkosi",
        "Abebe", "Bekele", "Osei", "Adeyemi", "Afolabi", "Kone", "Toure", "Diallo", "Keita", "Traore",
        // Indian surnames
        "Patel", "Kumar", "Singh", "Shah", "Desai", "Sharma", "Verma", "Gupta", "Malhotra", "Kapoor",
        "Joshi", "Chopra", "Mehta", "Reddy", "Nair", "Menon", "Pillai", "Rao", "Mukherjee", "Chatterjee"
    };

    private static readonly string[] Occupations = {
        // Education - School
        "Elementary Teacher", "High School Teacher", "Math Teacher", "Science Teacher", "English Teacher",
        "History Teacher", "Art Teacher", "Music Teacher", "Physical Education Teacher", "Special Education Teacher",
        "School Principal", "School Counselor", "School Nurse", "School Librarian", "Teaching Assistant",
        "School Administrator", "School Psychologist", "Language Teacher", "Computer Science Teacher", "Student",
        
        // Education - College/University
        "Professor", "Associate Professor", "Assistant Professor", "Research Professor", "Department Chair",
        "Academic Advisor", "Dean", "Research Assistant", "Teaching Fellow", "Lab Technician",
        "University Librarian", "Student Affairs Coordinator", "Campus Security Officer", "Athletics Coach",
        "Academic Department Secretary", "Admissions Officer", "Financial Aid Advisor", "Career Counselor",
        
        // Office/Corporate
        "Software Engineer", "Project Manager", "Business Analyst", "Human Resources Manager", "Marketing Manager",
        "Financial Analyst", "Accountant", "Office Manager", "Administrative Assistant", "Executive Assistant",
        "IT Support Specialist", "Systems Administrator", "Database Administrator", "Network Engineer",
        "Sales Manager", "Customer Service Representative", "Operations Manager", "Product Manager",
        "Content Writer", "Graphic Designer", "UX Designer", "Web Developer", "Data Analyst",
        
        // Professional Services
        "Attorney", "Legal Secretary", "Paralegal", "Corporate Counsel", "Tax Consultant",
        "Management Consultant", "Investment Banker", "Financial Advisor", "Insurance Agent",
        "Real Estate Agent", "Property Manager", "Corporate Trainer", "Compliance Officer",
        
        // Healthcare (in educational/office settings)
        "School Nurse", "University Health Services Doctor", "Campus Counselor", "Occupational Therapist",
        "Physical Therapist", "Mental Health Counselor", "Healthcare Administrator", "Medical Records Clerk",
        
        // Support/Facilities
        "Facilities Manager", "Maintenance Supervisor", "Building Engineer", "Security Guard",
        "Custodial Supervisor", "Cafeteria Manager", "IT Help Desk Technician", "Office Coordinator",
        "Reception Desk Manager", "Mail Room Coordinator"
    };

    private static readonly string[] PersonalityTraits = {
        // Positive Professional Traits
        "organized", "detail-oriented", "punctual", "efficient", "professional", "methodical",
        "analytical", "strategic", "goal-oriented", "resourceful", "diplomatic", "collaborative",
        "decisive", "pragmatic", "systematic", "results-driven", "thorough", "disciplined",
        "focused", "reliable", "proactive", "accountable", "consistent", "diligent",
        
        // Negative Professional Traits
        "perfectionist", "workaholic", "inflexible", "overly-cautious", "bureaucratic",
        "risk-averse", "rigid", "process-obsessed", "change-resistant", "overly-analytical",
        "micromanaging", "approval-seeking", "indecisive", "overly-competitive",
        
        // Positive Leadership & Management
        "leadership-oriented", "mentoring", "delegating", "authoritative", "coaching-minded",
        "team-oriented", "influential", "motivating", "decisive", "strategic-thinking",
        "visionary", "objective", "fair-minded", "executive-minded",
        
        // Negative Leadership & Management
        "controlling", "authoritarian", "micro-managing", "dismissive", "credit-taking",
        "overly-critical", "indecisive", "inconsistent", "playing-favorites", "unassertive",
        
        // Positive Communication & Interpersonal
        "articulate", "communicative", "persuasive", "engaging", "clear-speaking",
        "good-listener", "diplomatic", "tactful", "networking-oriented", "collaborative",
        
        // Negative Communication & Interpersonal
        "long-winded", "interrupting", "gossip-prone", "overly-talkative", "poor-listener",
        "confrontational", "argumentative", "passive-aggressive", "overly-blunt", "withdrawn",
        
        // Positive Work Style
        "deadline-driven", "multi-tasking", "independent", "self-motivated", "initiative-taking",
        "structured", "process-oriented", "quality-focused", "detail-conscious",
        
        // Negative Work Style
        "procrastinating", "disorganized", "easily-distracted", "deadline-missing",
        "scattered", "inconsistent", "corner-cutting", "unfocused", "time-wasting",
        
        // Positive Teaching & Mentoring
        "patient", "instructive", "encouraging", "nurturing", "explanatory",
        "guiding", "inspiring", "educational", "developmental", "supportive",
        
        // Negative Teaching & Mentoring
        "condescending", "impatient", "unclear", "intimidating", "discouraging",
        "overly-strict", "inflexible", "unapproachable", "playing-favorites",
        
        // Positive Problem-Solving
        "analytical", "problem-solving", "logical", "systematic", "investigative",
        "research-oriented", "data-driven", "technical", "methodical",
        
        // Negative Problem-Solving
        "analysis-paralysis", "overly-complicated", "tunnel-visioned", "indecisive",
        "overly-cautious", "nitpicking", "pessimistic", "criticism-focused",
        
        // Positive Service & Support
        "helpful", "service-oriented", "accommodating", "responsive", "customer-focused",
        "supportive", "assisting", "considerate", "understanding",
        
        // Negative Service & Support
        "unhelpful", "dismissive", "unresponsive", "impatient", "inflexible",
        "bureaucratic", "by-the-book", "unsympathetic", "easily-frustrated",
        
        // Positive Emotional & Social
        "emotionally-intelligent", "empathetic", "culturally-aware", "socially-perceptive",
        "self-aware", "composed", "stress-tolerant", "conflict-resolving",
        
        // Negative Emotional & Social
        "easily-stressed", "moody", "temperamental", "thin-skinned", "defensive",
        "easily-offended", "conflict-avoiding", "overly-sensitive", "dramatic",
        
        // Positive Ethics & Integrity
        "ethical", "principled", "honest", "trustworthy", "responsible",
        "conscientious", "reliable", "dedicated", "committed",
        
        // Negative Ethics & Integrity
        "credit-stealing", "blame-shifting", "responsibility-avoiding", "excuse-making",
        "corner-cutting", "rule-bending", "deadline-missing", "unreliable"
    };

    private static readonly string[] Pronouns = {
        "he/him", "she/her", "they/them"
    };

    private static readonly string[] Roles = {
        // Student roles
        "High School Student", "College Student", "Graduate Student", "Exchange Student", "Student Council Member",
        "Student Athlete", "Student Tutor", "Student Researcher", "Student Volunteer", "Student Club Leader",
        
        // Education roles
        "Elementary Teacher", "High School Teacher", "Professor", "Teaching Assistant", "School Counselor",
        "School Principal", "Academic Advisor", "School Librarian", "Education Consultant", "Special Education Teacher",
        
        // Office/Corporate roles
        "Office Manager", "Administrative Assistant", "Project Manager", "Software Developer", "Marketing Specialist",
        "Human Resources Manager", "Financial Analyst", "Customer Service Representative", "Sales Representative",
        "IT Support Specialist", "Data Analyst", "Business Consultant", "Executive Assistant"
    };

    private static readonly string[] EmotionalDispositions = {
        "generally optimistic", "quietly optimistic", "cautiously optimistic", "persistently cheerful", "calmly content",
        "typically reserved", "thoughtfully introspective", "carefully guarded", "openly emotional", "passionately expressive",
        "easily excitable", "steadily composed", "naturally empathetic", "analytically detached", "warmly compassionate",
        "occasionally melancholic", "pragmatically realistic", "habitually anxious", "confidently self-assured", "charmingly enthusiastic"
    };

    private static readonly string[] MotivationsGoals = {
        "advance in career", "help others succeed", "learn new skills", "make meaningful connections", "achieve work-life balance",
        "gain recognition in field", "contribute to community", "solve challenging problems", "create innovative solutions",
        "mentor younger colleagues", "build financial security", "travel and explore", "start a family", "develop expertise",
        "get into a good college", "graduate with honors", "find their passion", "make a difference", "understand the world",
        "improve systems", "advocate for change", "preserve traditions", "break new ground", "overcome personal challenges"
    };

    private static readonly string[] CommunicationStyles = {
        "direct and concise", "thoughtful and reserved", "warm and engaging", "analytical and precise", "enthusiastic and expressive",
        "diplomatic and tactful", "straightforward and honest", "patient and explanatory", "formal and professional",
        "casual and approachable", "prefers text over voice", "visual communicator", "active listener", "detail-oriented",
        "big-picture focused", "story-driven", "data-backed", "question-oriented", "collaborative", "authoritative"
    };

    private static readonly string[] KnowledgeScopes = {
        "mathematics and statistics", "literature and writing", "history and politics", "art and design", "music and performance",
        "biology and chemistry", "physics and astronomy", "computer science and programming", "psychology and sociology",
        "business and economics", "engineering and mechanics", "languages and linguistics", "philosophy and ethics",
        "environmental science", "medicine and health", "law and governance", "sports and fitness", "cooking and nutrition",
        "media and communications", "technology and gadgets", "crafts and DIY projects", "pop culture and entertainment"
    };

    private static readonly string[] BackstoryTemplates = {
        "{0} is a {1} at {2}, passionate about {3}. {4} {5} through {6}.",
        "Originally from {0}, {1} moved to pursue {2}. {3} {4} and hopes to {5}.",
        "After {0}, {1} decided to {2}. Now {3} while balancing {4} and {5}.",
        "{0} grew up in {1} and developed an interest in {2} at an early age. {3} {4} and is known for {5}.",
        "{0} has spent {1} years as a {2}. {3} {4} and is particularly skilled at {5}."
    };

    private static readonly string[] Locations = {
        "Oak Ridge High", "Westlake Academy", "Riverdale School", "Metro University", "Bayside College",
        "Greenfield Elementary", "Tech Institute", "City Hospital", "Central Office", "Downtown Firm",
        "Regional Headquarters", "Eastside Community Center", "Northern Branch", "Main Campus", "Southside Clinic"
    };

    private static readonly string[] Interests = {
        "science", "literature", "history", "art", "music", "sports", "technology", "politics",
        "environmental issues", "social justice", "mathematics", "engineering", "medicine", "business",
        "languages", "cooking", "travel", "photography", "film", "theater", "dance", "philosophy"
    };

    private static readonly string[] Challenges = {
        "has overcome significant challenges", "has struggled with social anxiety", "faced academic difficulties",
        "dealt with family responsibilities", "balanced multiple commitments", "navigated cultural differences",
        "managed health issues", "worked through personal loss", "adapted to major life changes",
        "persevered through financial hardship", "recovered from setbacks", "pushed through self-doubt"
    };

    private static readonly string[] Achievements = {
        "found success", "excelled academically", "built strong relationships", "developed unique skills",
        "gained recognition", "found confidence", "discovered new passions", "made valuable contributions",
        "achieved personal goals", "earned respect from peers", "created meaningful work", "made lasting impact"
    };

    private static readonly string[] Activities = {
        "participating in competitions", "volunteering in the community", "leading group projects",
        "tutoring others", "creating original work", "organizing events", "researching independently",
        "joining specialized programs", "connecting with mentors", "collaborating with teams",
        "presenting at conferences", "publishing their ideas", "developing new initiatives"
    };

    public enum GenerationType
    {
        Random,
        Uniform
    }

    public static Persona GeneratePersona(GenerationType type = GenerationType.Random)
    {
        if (type == GenerationType.Random)
        {
            return GenerateRandomPersona();
        }
        else
        {
            return GenerateUniformPersona();
        }
    }

    private static Persona GenerateRandomPersona()
    {
        bool isMale = Random.value > 0.5f;
        bool isNonBinary = Random.value > 0.9f;
        
        string firstName = isMale && !isNonBinary ? 
            MaleNames[Random.Range(0, MaleNames.Length)] : 
            FemaleNames[Random.Range(0, FemaleNames.Length)];
        string lastName = Surnames[Random.Range(0, Surnames.Length)];
        string fullName = $"{firstName} {lastName}";

        string gender = isNonBinary ? "Nonbinary" : (isMale ? "Male" : "Female");
        string pronouns = isNonBinary ? "they/them" : (isMale ? "he/him" : "she/her");
        
        string role = Roles[Random.Range(0, Roles.Length)];
        
        List<string> traits = GenerateRandomPersonality();
        string personalityTraits = string.Join(", ", traits);
        
        string emotionalDisposition = EmotionalDispositions[Random.Range(0, EmotionalDispositions.Length)];
        
        // Select 2-3 motivations/goals
        List<string> motivations = new List<string>();
        int motivationCount = Random.Range(2, 4);
        List<string> availableMotivations = new List<string>(MotivationsGoals);
        
        for (int i = 0; i < motivationCount; i++)
        {
            if (availableMotivations.Count == 0) break;
            int index = Random.Range(0, availableMotivations.Count);
            motivations.Add(availableMotivations[index]);
            availableMotivations.RemoveAt(index);
        }
        string motivationsGoals = string.Join(", ", motivations);
        
        string communicationStyle = CommunicationStyles[Random.Range(0, CommunicationStyles.Length)];
        
        // Select 2-3 knowledge areas
        List<string> knowledgeAreas = new List<string>();
        int knowledgeCount = Random.Range(2, 4);
        List<string> availableKnowledge = new List<string>(KnowledgeScopes);
        
        for (int i = 0; i < knowledgeCount; i++)
        {
            if (availableKnowledge.Count == 0) break;
            int index = Random.Range(0, availableKnowledge.Count);
            knowledgeAreas.Add(availableKnowledge[index]);
            availableKnowledge.RemoveAt(index);
        }
        string knowledgeScope = string.Join(", ", knowledgeAreas);
        
        string backstory = GenerateBackstory(fullName, gender, role);
        
        Persona persona = new Persona
        {
            name = fullName,
            role = role,
            age = Random.Range(20, 65).ToString(),
            gender = gender,
            pronouns = pronouns,
            personality_traits = personalityTraits,
            emotional_disposition = emotionalDisposition,
            motivations_goals = motivationsGoals,
            communication_style = communicationStyle,
            knowledge_scope = knowledgeScope,
            backstory = backstory
        };

        return persona;
    }

    private static Persona GenerateUniformPersona()
    {
        bool isMale = Random.value > 0.5f;
        bool isNonBinary = Random.value > 0.9f;
        
        string firstName = isMale && !isNonBinary ? 
            MaleNames[Random.Range(0, MaleNames.Length)] : 
            FemaleNames[Random.Range(0, FemaleNames.Length)];
        string lastName = Surnames[Random.Range(0, Surnames.Length)];
        string fullName = $"{firstName} {lastName}";
        
        string gender = isNonBinary ? "Nonbinary" : (isMale ? "Male" : "Female");
        string pronouns = isNonBinary ? "they/them" : (isMale ? "he/him" : "she/her");
        
        Persona persona = new Persona
        {
            name = fullName,
            role = "Student",
            age = "21",
            gender = gender,
            pronouns = pronouns,
            personality_traits = "balanced, friendly, studious",
            emotional_disposition = "generally positive",
            motivations_goals = "complete education, make friends, develop skills",
            communication_style = "clear and direct",
            knowledge_scope = "general academics, social media, current events",
            backstory = $"{firstName} is a typical college student focused on their studies while enjoying campus life. They maintain a good balance between academics and social activities."
        };

        return persona;
    }

    private static List<string> GenerateRandomPersonality()
    {
        List<string> traits = new List<string>();
        int traitCount = Random.Range(3, 6);
        
        List<string> availableTraits = new List<string>(PersonalityTraits);
        
        for (int i = 0; i < traitCount; i++)
        {
            if (availableTraits.Count == 0) break;
            
            int index = Random.Range(0, availableTraits.Count);
            traits.Add(availableTraits[index]);
            availableTraits.RemoveAt(index);
        }
        
        return traits;
    }

    private static string GenerateBackstory(string name, string gender, string role)
    {
        string template = BackstoryTemplates[Random.Range(0, BackstoryTemplates.Length)];
        string pronoun = gender == "Nonbinary" ? "They" : (gender == "Male" ? "He" : "She");
        string possessive = gender == "Nonbinary" ? "their" : (gender == "Male" ? "his" : "her");
        
        string location = Locations[Random.Range(0, Locations.Length)];
        string interest = Interests[Random.Range(0, Interests.Length)];
        string challenge = Challenges[Random.Range(0, Challenges.Length)];
        string achievement = Achievements[Random.Range(0, Achievements.Length)];
        string activity = Activities[Random.Range(0, Activities.Length)];
        
        switch (Random.Range(0, 5))
        {
            case 0:
                return string.Format(template,
                    name,
                    role.ToLower(),
                    location,
                    interest,
                    pronoun,
                    achievement,
                    activity);
            
            case 1:
                string[] cities = { "Boston", "Chicago", "San Francisco", "New York", "Seattle", "Austin", "Los Angeles", "Miami", "Atlanta", "Houston" };
                return string.Format(template,
                    cities[Random.Range(0, cities.Length)],
                    name,
                    role.ToLower(),
                    pronoun,
                    challenge,
                    MotivationsGoals[Random.Range(0, MotivationsGoals.Length)]);
            
            case 2:
                string[] pastEvents = { "graduating high school", "a career change", "moving to a new city", "volunteering abroad", "a personal revelation" };
                return string.Format(template,
                    pastEvents[Random.Range(0, pastEvents.Length)],
                    name,
                    "pursue " + interest,
                    pronoun + " works as a " + role.ToLower(),
                    possessive + " " + interest,
                    "family responsibilities");
            
            case 3:
                string[] environments = { "a small town", "the city", "a diverse community", "a traditional household", "various places around the country" };
                return string.Format(template,
                    name,
                    environments[Random.Range(0, environments.Length)],
                    interest,
                    pronoun,
                    challenge,
                    possessive + " " + KnowledgeScopes[Random.Range(0, KnowledgeScopes.Length)]);
            
            default:
                string[] timeframes = { "two", "three", "four", "five", "several", "many" };
                return string.Format(template,
                    name,
                    timeframes[Random.Range(0, timeframes.Length)],
                    role.ToLower(),
                    pronoun,
                    "is passionate about " + interest,
                    activity);
        }
    }
}