import {
    useCallback,
    useEffect,
    useMemo,
    useRef,
    useState,
    type FormEvent,
    type ReactNode,
} from "react";
import axios from "axios";
import {
    Activity,
    AlertCircle,
    ArrowDownRight,
    ArrowRight,
    Bell,
    CalendarDays,
    Check,
    ChevronDown,
    CircleHelp,
    Clock3,
    Command,
    FolderKanban,
    LayoutDashboard,
    ListTodo,
    LoaderCircle,
    LogOut,
    Menu,
    Plus,
    RefreshCw,
    Search,
    Settings,
    ShieldCheck,
    Sparkles,
    Users,
    X,
    Zap,
} from "lucide-react";
import "./index.css";

const API_BASE_URL = "https://localhost:7149/api";
const TOKEN_KEY = "projectflow_token";
const USER_KEY = "projectflow_user";
const TENANT_KEY = "projectflow_tenant";

const api = axios.create({ baseURL: API_BASE_URL });

api.interceptors.request.use((config) => {
    const token = localStorage.getItem(TOKEN_KEY);
    const tenant = localStorage.getItem(TENANT_KEY) || "globex";

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    config.headers["X-Tenant-Slug"] = tenant;
    return config;
});

type User = {
    id?: string;
    userId?: string;
    userName?: string;
    displayName?: string;
    email?: string;
};

type AuthResponse = {
    token?: string;
    accessToken?: string;
    user?: User;
    id?: string;
    userId?: string;
    userName?: string;
    displayName?: string;
    email?: string;
};

type Project = {
    id: string;
    name: string;
    description?: string | null;
    dueDateUtc?: string | null;
    createdAtUtc?: string;
    updatedAtUtc?: string | null;
    createdAt?: string;
};

type WorkItem = {
    id: string;
    projectId?: string;
    title: string;
    description?: string | null;
    status?: string | number;
    priority?: string | number;
    dueDateUtc?: string | null;
    dueDate?: string | null;
    createdAtUtc?: string;
};

type Member = {
    id: string;
    userId?: string;
    userName?: string;
    displayName?: string;
    email?: string;
    role?: string | number;
    createdAtUtc?: string;
};

type AvailableUser = {
    userId: string;
    displayName?: string | null;
    userName?: string | null;
    email?: string | null;
};

type NotificationItem = {
    id: string;
    title: string;
    message: string;
    createdAtUtc?: string;
    isRead?: boolean;
    read?: boolean;
};

type ActivityItem = {
    id: string;
    details?: string;
    type?: string | number;
    actorUserId?: string;
    description?: string;
    message?: string;
    action?: string;
    userName?: string;
    createdAtUtc?: string;
};

type WorkItemComment = {
    id: string;
    workItemId: string;
    authorUserId: string;
    content: string;
    createdAtUtc: string;
    updatedAtUtc?: string | null;
};

type WorkItemAssignment = {
    id: string;
    workItemId: string;
    userId: string;
    createdAtUtc: string;
};

type Page =
    | "Overview"
    | "Projects"
    | "My tasks"
    | "Team"
    | "Notifications"
    | "Settings";

type Notice = {
    id: number;
    message: string;
    type: "success" | "error";
} | null;

type ModalType = "project" | "task" | "taskDetails" | "member" | null;

function readStoredUser(): User | null {
    try {
        const stored = localStorage.getItem(USER_KEY);
        return stored ? (JSON.parse(stored) as User) : null;
    } catch {
        return null;
    }
}

function asArray<T>(data: unknown): T[] {
    if (Array.isArray(data)) return data as T[];

    if (data && typeof data === "object") {
        const object = data as Record<string, unknown>;

        for (const key of ["items", "data", "results", "value"]) {
            if (Array.isArray(object[key])) return object[key] as T[];
        }
    }

    return [];
}

function errorMessage(error: unknown): string {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data as
            | {
                message?: string;
                title?: string;
                detail?: string;
                errors?: Record<string, string[]>;
            }
            | string
            | undefined;

        if (typeof data === "string" && data.trim()) return data;

        if (data && typeof data === "object") {
            if (data.message) return data.message;
            if (data.detail) return data.detail;
            if (data.title) return data.title;
            if (data.errors) {
                return Object.values(data.errors).flat().join(" ");
            }
        }

        if (error.response?.status === 401) {
            return "Your session may have expired. Please sign in again.";
        }

        if (error.response?.status === 403) {
            return "You don't have permission to perform this action.";
        }

        if (!error.response) {
            return "Couldn't reach the API. Check that the backend is running and HTTPS is trusted.";
        }
    }

    return error instanceof Error ? error.message : "Something went wrong.";
}

function titleCase(value: string): string {
    return value
        .replace(/([a-z])([A-Z])/g, "$1 $2")
        .replace(/[-_]/g, " ")
        .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

function statusLabel(status: WorkItem["status"]): string {
    if (typeof status === "number") {
        return ["To do", "In progress", "In review", "Completed"][status - 1] ?? "To do";
    }

    const label = titleCase(String(status || "To do"));
    return label.toLowerCase() === "done" ? "Completed" : label;
}

function priorityLabel(priority: WorkItem["priority"]): string {
    if (typeof priority === "number") {
        return ["Low", "Medium", "High", "Critical"][priority - 1] ?? "Medium";
    }

    const label = titleCase(String(priority || "Medium"));
    return label.toLowerCase() === "urgent" ? "Critical" : label;
}

function statusValue(value: string): number {
    return ({ "To do": 1, "In progress": 2, "In review": 3, Completed: 4 })[value] || 1;
}

function priorityValue(value: string): number {
    return ({ Low: 1, Medium: 2, High: 3, Critical: 4 })[value] || 2;
}

function roleLabel(role: Member["role"]): string {
    if (typeof role === "number") return role === 2 ? "Manager" : "Member";
    return titleCase(String(role || "Member"));
}

function formatDate(value?: string | null): string {
    if (!value) return "No due date";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "No due date";

    return new Intl.DateTimeFormat(undefined, {
        month: "short",
        day: "numeric",
        year:
            date.getFullYear() !== new Date().getFullYear()
                ? "numeric"
                : undefined,
    }).format(date);
}

function relativeTime(value?: string): string {
    if (!value) return "Recently";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "Recently";

    const minutes = Math.max(
        0,
        Math.floor((Date.now() - date.getTime()) / 60_000),
    );

    if (minutes < 1) return "Just now";
    if (minutes < 60) return `${minutes}m ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;

    const days = Math.floor(hours / 24);
    if (days < 7) return `${days}d ago`;

    return formatDate(value);
}

function userName(user?: User | null): string {
    return (
        user?.displayName ||
        user?.userName ||
        user?.email?.split("@")[0] ||
        "Workspace member"
    );
}

function initials(value?: string | null): string {
    return (value || "U")
        .trim()
        .split(/\s+/)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase() || "")
        .join("");
}

function isDone(item: WorkItem): boolean {
    return statusLabel(item.status).toLowerCase() === "completed";
}

function Avatar({
    name,
    size = "md",
}: {
    name?: string | null;
    size?: "sm" | "md" | "lg";
}) {
    return (
        <span className={`avatar avatar-${size}`} aria-label={name || "User"}>
            {initials(name)}
        </span>
    );
}

function IconButton({
    children,
    label,
    onClick,
    disabled = false,
}: {
    children: ReactNode;
    label: string;
    onClick?: () => void;
    disabled?: boolean;
}) {
    return (
        <button
            type="button"
            className="icon-button"
            aria-label={label}
            title={label}
            onClick={onClick}
            disabled={disabled}
        >
            {children}
        </button>
    );
}

function EmptyState({
    icon,
    title,
    description,
    action,
}: {
    icon: ReactNode;
    title: string;
    description: string;
    action?: ReactNode;
}) {
    return (
        <div className="empty-state">
            <div className="empty-state-icon">{icon}</div>
            <h3>{title}</h3>
            <p>{description}</p>
            {action}
        </div>
    );
}

function Modal({
    title,
    description,
    onClose,
    children,
}: {
    title: string;
    description?: string;
    onClose: () => void;
    children: ReactNode;
}) {
    useEffect(() => {
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") onClose();
        };

        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [onClose]);

    return (
        <div
            className="modal-backdrop"
            onMouseDown={(event) => {
                if (event.target === event.currentTarget) onClose();
            }}
        >
            <section
                className="modal-card"
                role="dialog"
                aria-modal="true"
                aria-labelledby="modal-title"
            >
                <header className="modal-header">
                    <div>
                        <h2 id="modal-title">{title}</h2>
                        {description && <p>{description}</p>}
                    </div>
                    <IconButton label="Close dialog" onClick={onClose}>
                        <X size={18} />
                    </IconButton>
                </header>
                {children}
            </section>
        </div>
    );
}

function StatCard({
    label,
    value,
    caption,
    icon,
    tone,
}: {
    label: string;
    value: number;
    caption: string;
    icon: ReactNode;
    tone: string;
}) {
    return (
        <article className="stat-card">
            <div className="stat-card-top">
                <span className={`stat-icon ${tone}`}>{icon}</span>
                <span className="stat-decoration">
                    <ArrowDownRight size={16} />
                </span>
            </div>
            <div className="stat-value">{value}</div>
            <div className="stat-label">{label}</div>
            <p className="stat-caption">{caption}</p>
        </article>
    );
}

function TaskTable({
    items,
    projects,
    onStatusChange,
    onPriorityChange,
    onOpen,
}: {
    items: WorkItem[];
    projects: Project[];
    onStatusChange: (item: WorkItem, status: number) => void;
    onPriorityChange: (item: WorkItem, priority: number) => void;
    onOpen: (item: WorkItem) => void;
}) {
    if (!items.length) {
        return (
            <EmptyState
                icon={<ListTodo size={22} />}
                title="No tasks match"
                description="Try another search or create a task to get started."
            />
        );
    }

    return (
        <div className="table-scroll">
            <table className="task-table">
                <thead>
                    <tr>
                        <th>Task</th>
                        <th>Project</th>
                        <th>Status</th>
                        <th>Priority</th>
                        <th>Due date</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map((item) => {
                        const status = statusLabel(item.status);
                        const priority = priorityLabel(item.priority);
                        const project = projects.find(
                            (candidate) => candidate.id === item.projectId,
                        );

                        return (
                            <tr key={item.id}>
                                <td>
                                    <div className="task-title">
                                        <span
                                            className={`task-marker ${isDone(item) ? "task-marker-done" : ""
                                                }`}
                                        >
                                            {isDone(item) && <Check size={12} />}
                                        </span>
                                        <div>
                                            <button
                                                type="button"
                                                className="text-button"
                                                onClick={() => onOpen(item)}
                                            >
                                                {item.title}
                                            </button>
                                            {item.description && <small>{item.description}</small>}
                                        </div>
                                    </div>
                                </td>
                                <td>
                                    <span className="project-inline">
                                        <span className="project-color-dot" />
                                        {project?.name || "Project"}
                                    </span>
                                </td>
                                <td>
                                    <select
                                        className={`status-pill status-${status === "Completed" ? "done" : status.toLowerCase().replace(/\s+/g, "-")}`}
                                        aria-label={`Change status for ${item.title}`}
                                        value={typeof item.status === "number" ? item.status : statusValue(status)}
                                        onChange={(event) => onStatusChange(item, Number(event.target.value))}
                                    >
                                        <option value={1}>To do</option>
                                        <option value={2}>In progress</option>
                                        <option value={3}>In review</option>
                                        <option value={4}>Completed</option>
                                    </select>
                                </td>
                                <td>
                                    <select
                                        className={`priority-pill priority-${priority.toLowerCase()}`}
                                        aria-label={`Change priority for ${item.title}`}
                                        value={typeof item.priority === "number" ? item.priority : priorityValue(priority)}
                                        onChange={(event) => onPriorityChange(item, Number(event.target.value))}
                                    >
                                        <option value={1}>Low</option>
                                        <option value={2}>Medium</option>
                                        <option value={3}>High</option>
                                        <option value={4}>Critical</option>
                                    </select>
                                </td>
                                <td>
                                    <span className="date-cell">
                                        <CalendarDays size={14} />
                                        {formatDate(item.dueDateUtc || item.dueDate)}
                                    </span>
                                </td>
                            </tr>
                        );
                    })}
                </tbody>
            </table>
        </div>
    );
}

function ActivityRow({ item }: { item: ActivityItem }) {
    return (
        <article className="activity-row">
            <Avatar name={item.userName || "Team"} size="sm" />
            <div className="activity-copy">
                <p>{item.details || item.description || item.message || item.action || "Project updated"}</p>
                <small>{relativeTime(item.createdAtUtc)}</small>
            </div>
            <span className="activity-timeline-dot" />
        </article>
    );
}

export default function App() {
    const [token, setToken] = useState(
        () => localStorage.getItem(TOKEN_KEY) || "",
    );
    const [user, setUser] = useState<User | null>(readStoredUser);
    const [tenant, setTenant] = useState(
        () => localStorage.getItem(TENANT_KEY) || "globex",
    );

    const [page, setPage] = useState<Page>("Overview");
    const [authMode, setAuthMode] = useState<"login" | "register">("login");
    const [authForm, setAuthForm] = useState({
        displayName: "",
        userName: "",
        email: "",
        password: "",
    });
    const [selectedTask, setSelectedTask] = useState<WorkItem | null>(null);
    const [taskComments, setTaskComments] = useState<WorkItemComment[]>([]);
    const [taskAssignments, setTaskAssignments] = useState<WorkItemAssignment[]>([]);
    const [commentDraft, setCommentDraft] = useState("");
    const [assignmentUserId, setAssignmentUserId] = useState("");
    const [loadingTaskDetails, setLoadingTaskDetails] = useState(false);
    const [tenantDraft, setTenantDraft] = useState(tenant);
    const [authLoading, setAuthLoading] = useState(false);
    const [authError, setAuthError] = useState("");

    const [projects, setProjects] = useState<Project[]>([]);
    const [workItems, setWorkItems] = useState<WorkItem[]>([]);
    const [members, setMembers] = useState<Member[]>([]);
    const [membersProjectId, setMembersProjectId] = useState("");
    const [activities, setActivities] = useState<ActivityItem[]>([]);
    const [activitiesProjectId, setActivitiesProjectId] = useState("");
    const [notifications, setNotifications] = useState<NotificationItem[]>([]);

    const [loading, setLoading] = useState(Boolean(token));
    const [refreshKey, setRefreshKey] = useState(0);
    const [notice, setNotice] = useState<Notice>(null);
   // const [noticeCounter, setNoticeCounter] = useState(0);
    const [search, setSearch] = useState("");
    const [taskStatusFilter, setTaskStatusFilter] = useState("All");
    const [taskPriorityFilter, setTaskPriorityFilter] = useState("All");
    const [activeProjectId, setActiveProjectId] = useState("");
    const [mobileNavOpen, setMobileNavOpen] = useState(false);

    const [modal, setModal] = useState<ModalType>(null);
    const [saving, setSaving] = useState(false);
    const [projectForm, setProjectForm] = useState({
        name: "",
        description: "",
    });
    const [taskForm, setTaskForm] = useState({
        title: "",
        description: "",
        projectId: "",
        priority: "Medium",
        dueDateUtc: "",
    });
    const [memberForm, setMemberForm] = useState({ role: "Member" });
    const [memberSearch, setMemberSearch] = useState("");
    const [availableUsers, setAvailableUsers] = useState<AvailableUser[]>([]);
    const [selectedUser, setSelectedUser] = useState<AvailableUser | null>(null);
    const [searchingUsers, setSearchingUsers] = useState(false);
    const [memberSearchError, setMemberSearchError] = useState("");

    const activeProject =
        projects.find((project) => project.id === activeProjectId) ||
        projects[0] ||
        null;

    const displayName = userName(user);
    const currentUserId = user?.id || user?.userId;
    const tenantLabel = titleCase(tenant);
    const canManageSelectedTask = Boolean(
        selectedTask?.projectId &&
        membersProjectId === selectedTask.projectId &&
        members.some((member) =>
            member.userId === currentUserId && roleLabel(member.role) === "Manager",
        ),
    );
    const unreadCount = notifications.filter(
        (item) => !(item.isRead ?? item.read ?? false),
    ).length;

    const noticeCounter = useRef(0);

    const notify = useCallback(
        (message: string, type: "success" | "error" = "success") => {
            noticeCounter.current += 1;

            setNotice({
                id: noticeCounter.current,
                message,
                type,
            });
        },
        [],
    );

    useEffect(() => {
        if (!notice) return;

        const timeout = window.setTimeout(() => {
            setNotice((current) => (current?.id === notice.id ? null : current));
        }, 4000);

        return () => window.clearTimeout(timeout);
    }, [notice]);

    // Initial workspace load and manual refresh. State updates happen after
    // asynchronous work, not synchronously inside the effect body.
    useEffect(() => {
        if (!token) return;

        let cancelled = false;

        async function loadWorkspace() {
            try {
                const projectResponse = await api.get("/projects");
                const projectList = asArray<Project>(projectResponse.data);

                if (cancelled) return;

                setProjects(projectList);
                setActiveProjectId((current) =>
                    projectList.some((project) => project.id === current)
                        ? current
                        : projectList[0]?.id || "",
                );

                const taskResults = await Promise.allSettled(
                    projectList.map((project) =>
                        api.get(`/work-items/project/${project.id}`),
                    ),
                );

                if (cancelled) return;

                const allTasks = taskResults.flatMap((result) =>
                    result.status === "fulfilled"
                        ? asArray<WorkItem>(result.value.data)
                        : [],
                );

                setWorkItems([
                    ...new Map(allTasks.map((item) => [item.id, item])).values(),
                ]);

                try {
                    const notificationResponse = await api.get("/notifications");
                    if (!cancelled) {
                        setNotifications(asArray<NotificationItem>(notificationResponse.data));
                    }
                } catch {
                    // Notifications are optional for the rest of the workspace.
                    if (!cancelled) setNotifications([]);
                }
            } catch (error) {
                if (!cancelled) notify(errorMessage(error), "error");
            } finally {
                if (!cancelled) setLoading(false);
            }
        }

        void loadWorkspace();

        return () => {
            cancelled = true;
        };
    }, [token, refreshKey, notify]);

    // Load project-specific information whenever the selected project changes.
    useEffect(() => {
        if (!token || !activeProject?.id) return;

        let cancelled = false;
        const projectId = activeProject.id;

        async function loadProjectDetails() {
            const [memberResult, activityResult] = await Promise.allSettled([
                api.get(`/projects/${projectId}/members`),
                api.get(`/projects/${projectId}/activities`),
            ]);

            if (cancelled) return;

            if (memberResult.status === "fulfilled") {
                setMembers(asArray<Member>(memberResult.value.data));
                setMembersProjectId(projectId);
            }

            if (activityResult.status === "fulfilled") {
                setActivities(asArray<ActivityItem>(activityResult.value.data));
                setActivitiesProjectId(projectId);
            }
        }

        void loadProjectDetails();

        return () => {
            cancelled = true;
        };
    }, [token, activeProject?.id, refreshKey]);

    // Search is debounced. Search resets happen in input/modal handlers, not
    // synchronously inside this effect.
    useEffect(() => {
        const query = memberSearch.trim();

        if (
            modal !== "member" ||
            !activeProject?.id ||
            query.length < 2 ||
            selectedUser
        ) {
            return;
        }

        let cancelled = false;

        const timeout = window.setTimeout(async () => {
            setSearchingUsers(true);
            setMemberSearchError("");

            try {
                const response = await api.get(
                    `/projects/${activeProject.id}/members/available-users`,
                    { params: { q: query } },
                );

                if (!cancelled) {
                    setAvailableUsers(asArray<AvailableUser>(response.data));
                }
            } catch (error) {
                if (!cancelled) {
                    setAvailableUsers([]);
                    setMemberSearchError(errorMessage(error));
                }
            } finally {
                if (!cancelled) setSearchingUsers(false);
            }
        }, 300);

        return () => {
            cancelled = true;
            window.clearTimeout(timeout);
        };
    }, [modal, activeProject?.id, memberSearch, selectedUser]);

    const filteredProjects = useMemo(() => {
        const query = search.trim().toLowerCase();

        return projects.filter((project) =>
            `${project.name} ${project.description || ""}`
                .toLowerCase()
                .includes(query),
        );
    }, [projects, search]);

    const filteredTasks = useMemo(() => {
        const query = search.trim().toLowerCase();

        return workItems.filter((item) => {
            const matchesQuery = `${item.title} ${item.description || ""} ${statusLabel(item.status)} ${priorityLabel(item.priority)}`
                .toLowerCase()
                .includes(query);

            const matchesStatus =
                taskStatusFilter === "All" ||
                statusLabel(item.status).toLowerCase() === taskStatusFilter.toLowerCase();

            const matchesPriority =
                taskPriorityFilter === "All" ||
                priorityLabel(item.priority).toLowerCase() ===
                taskPriorityFilter.toLowerCase();

            return matchesQuery && matchesStatus && matchesPriority;
        });
    }, [workItems, search, taskStatusFilter, taskPriorityFilter]);

    const openTasks = workItems.filter((item) => !isDone(item));
    const completedTasks = workItems.filter(isDone);
    const inProgressTasks = workItems.filter(
        (item) => statusLabel(item.status).toLowerCase() === "in progress",
    );
    const projectTasks = workItems.filter(
        (item) => item.projectId === activeProject?.id,
    );
    const projectCompleted = projectTasks.filter(isDone).length;

    async function submitAuth(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setAuthLoading(true);
        setAuthError("");

        try {
            const tenantSlug = tenant.trim().toLowerCase();
            if (!/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(tenantSlug)) {
                throw new Error("Enter a valid workspace slug using letters, numbers, and hyphens.");
            }

            localStorage.setItem(TENANT_KEY, tenantSlug);
            setTenant(tenantSlug);
            const endpoint =
                authMode === "login" ? "/auth/login" : "/auth/register";

            const payload =
                authMode === "login"
                    ? {
                        email: authForm.email.trim(),
                        password: authForm.password,
                    }
                    : {
                        userName: authForm.userName.trim(),
                        displayName: authForm.displayName.trim(),
                        email: authForm.email.trim(),
                        password: authForm.password,
                    };

            const response = await api.post<AuthResponse>(endpoint, payload);
            const data = response.data;
            const receivedToken = data.token || data.accessToken;

            if (!receivedToken) {
                throw new Error(
                    "The API response did not include a token. Check the auth response contract.",
                );
            }

            const receivedUser: User = data.user || {
                id: data.id,
                userId: data.userId,
                userName: data.userName,
                displayName: data.displayName,
                email: data.email || authForm.email.trim(),
            };

            localStorage.setItem(TOKEN_KEY, receivedToken);
            localStorage.setItem(USER_KEY, JSON.stringify(receivedUser));

            setUser(receivedUser);
            setToken(receivedToken);
            setLoading(true);
            setPage("Overview");
            notify(authMode === "login" ? "Welcome back." : "Account created.");
        } catch (error) {
            setAuthError(errorMessage(error));
        } finally {
            setAuthLoading(false);
        }
    }

    function signOut() {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);

        setToken("");
        setUser(null);
        setProjects([]);
        setWorkItems([]);
        setMembers([]);
        setActivities([]);
        setNotifications([]);
        setSearch("");
        setActiveProjectId("");
        setPage("Overview");
    }

    function saveTenant(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        const tenantSlug = tenantDraft.trim().toLowerCase();
        if (!/^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(tenantSlug)) {
            notify("Use a valid workspace slug with letters, numbers, and hyphens.", "error");
            return;
        }

        localStorage.setItem(TENANT_KEY, tenantSlug);
        setTenant(tenantSlug);
        setTenantDraft(tenantSlug);
        setProjects([]);
        setWorkItems([]);
        setMembers([]);
        setActivities([]);
        setNotifications([]);
        setActiveProjectId("");
        setLoading(true);
        setRefreshKey((value) => value + 1);
        notify("Workspace changed.");
    }

    async function updateTaskStatus(item: WorkItem, status: number) {
        try {
            await api.patch(`/work-items/${item.id}/status`, { status });
            setWorkItems((current) => current.map((task) =>
                task.id === item.id ? { ...task, status } : task,
            ));
            setSelectedTask((current) => current?.id === item.id ? { ...current, status } : current);
            setRefreshKey((value) => value + 1);
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function updateTaskPriority(item: WorkItem, priority: number) {
        try {
            await api.patch(`/work-items/${item.id}/priority`, { priority });
            setWorkItems((current) => current.map((task) =>
                task.id === item.id ? { ...task, priority } : task,
            ));
            setSelectedTask((current) => current?.id === item.id ? { ...current, priority } : current);
            setRefreshKey((value) => value + 1);
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function markNotificationRead(item: NotificationItem) {
        try {
            await api.patch(`/notifications/${item.id}/read`);
            setNotifications((current) => current.map((notification) =>
                notification.id === item.id ? { ...notification, isRead: true } : notification,
            ));
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function changeMemberRole(member: Member, role: number) {
        if (!activeProject) return;

        try {
            await api.patch(`/projects/${activeProject.id}/members/${member.id}/role`, { role });
            setMembers((current) => current.map((person) =>
                person.id === member.id ? { ...person, role } : person,
            ));
            notify("Project role updated.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function removeMember(member: Member) {
        if (!activeProject || !window.confirm("Remove this person from the project?")) return;

        try {
            await api.delete(`/projects/${activeProject.id}/members/${member.id}`);
            setMembers((current) => current.filter((person) => person.id !== member.id));
            notify("Project member removed.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function openTaskDetails(item: WorkItem) {
        setSelectedTask(item);
        setTaskComments([]);
        setTaskAssignments([]);
        setCommentDraft("");
        setAssignmentUserId("");
        setLoadingTaskDetails(true);
        setModal("taskDetails");

        const [commentsResult, assignmentsResult, membersResult] = await Promise.allSettled([
            api.get(`/work-items/${item.id}/comments`),
            api.get(`/work-items/${item.id}/assignments`),
            item.projectId ? api.get(`/projects/${item.projectId}/members`) : Promise.resolve(null),
        ]);

        if (commentsResult.status === "fulfilled") {
            setTaskComments(asArray<WorkItemComment>(commentsResult.value.data));
        }
        if (assignmentsResult.status === "fulfilled") {
            setTaskAssignments(asArray<WorkItemAssignment>(assignmentsResult.value.data));
        }
        if (membersResult.status === "fulfilled" && membersResult.value && item.projectId) {
            setMembers(asArray<Member>(membersResult.value.data));
            setMembersProjectId(item.projectId);
        }
        const failed = [commentsResult, assignmentsResult].find((result) => result.status === "rejected");
        if (failed?.status === "rejected") notify(errorMessage(failed.reason), "error");
        setLoadingTaskDetails(false);
    }

    async function saveTaskDetails(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (!selectedTask?.title.trim()) return;

        setSaving(true);
        try {
            await api.put(`/work-items/${selectedTask.id}`, {
                title: selectedTask.title.trim(),
                description: selectedTask.description?.trim() || null,
                dueDateUtc: selectedTask.dueDateUtc
                    ? new Date(`${selectedTask.dueDateUtc.slice(0, 10)}T12:00:00`).toISOString()
                    : null,
            });
            setWorkItems((current) => current.map((item) => item.id === selectedTask.id ? selectedTask : item));
            setModal(null);
            setRefreshKey((value) => value + 1);
            notify("Task details saved.");
        } catch (error) {
            notify(errorMessage(error), "error");
        } finally {
            setSaving(false);
        }
    }

    async function deleteTask() {
        if (!selectedTask || !window.confirm(`Delete “${selectedTask.title}”?`)) return;

        try {
            await api.delete(`/work-items/${selectedTask.id}`);
            setWorkItems((current) => current.filter((item) => item.id !== selectedTask.id));
            setSelectedTask(null);
            setModal(null);
            setRefreshKey((value) => value + 1);
            notify("Task deleted.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function addTaskComment(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (!selectedTask || !commentDraft.trim()) return;

        try {
            const response = await api.post<WorkItemComment>(`/work-items/${selectedTask.id}/comments`, {
                content: commentDraft.trim(),
            });
            setTaskComments((current) => [...current, response.data]);
            setCommentDraft("");
            setRefreshKey((value) => value + 1);
            notify("Comment added.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function assignTask(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (!selectedTask || !assignmentUserId) return;

        try {
            await api.post(`/work-items/${selectedTask.id}/assignments`, { userId: assignmentUserId });
            const response = await api.get(`/work-items/${selectedTask.id}/assignments`);
            setTaskAssignments(asArray<WorkItemAssignment>(response.data));
            setAssignmentUserId("");
            notify("Task assigned.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function removeTaskAssignment(assignment: WorkItemAssignment) {
        if (!selectedTask) return;

        try {
            await api.delete(`/work-items/${selectedTask.id}/assignments/${assignment.id}`);
            setTaskAssignments((current) => current.filter((item) => item.id !== assignment.id));
            notify("Assignment removed.");
        } catch (error) {
            notify(errorMessage(error), "error");
        }
    }

    async function createProject(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (!projectForm.name.trim()) return;

        setSaving(true);

        try {
            await api.post("/projects", {
                name: projectForm.name.trim(),
                description: projectForm.description.trim() || null,
            });

            setProjectForm({ name: "", description: "" });
            setModal(null);
            setLoading(true);
            setRefreshKey((value) => value + 1);
            notify("Project created successfully.");
        } catch (error) {
            notify(errorMessage(error), "error");
        } finally {
            setSaving(false);
        }
    }

    async function createTask(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (!taskForm.title.trim() || !taskForm.projectId) return;

        setSaving(true);

        try {
            const response = await api.post<{ id?: string }>("/work-items", {
                title: taskForm.title.trim(),
                description: taskForm.description.trim() || null,
                projectId: taskForm.projectId,
                dueDateUtc: taskForm.dueDateUtc
                    ? new Date(`${taskForm.dueDateUtc}T12:00:00`).toISOString()
                    : null,
            });

            let priorityWarning = "";
            if (taskForm.priority !== "Medium") {
                if (response.data?.id) {
                    try {
                        await api.patch(`/work-items/${response.data.id}/priority`, {
                            priority: priorityValue(taskForm.priority),
                        });
                    } catch (error) {
                        priorityWarning = ` The task was created, but its priority was not updated: ${errorMessage(error)}`;
                    }
                } else {
                    priorityWarning = " The task was created, but the API did not return its ID, so the priority could not be updated.";
                }
            }

            setTaskForm({
                title: "",
                description: "",
                projectId: activeProject?.id || "",
                priority: "Medium",
                dueDateUtc: "",
            });

            setModal(null);
            setLoading(true);
            setRefreshKey((value) => value + 1);
            notify(`Task created successfully.${priorityWarning}`, priorityWarning ? "error" : "success");
        } catch (error) {
            notify(errorMessage(error), "error");
        } finally {
            setSaving(false);
        }
    }

    async function addMember(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (!activeProject || !selectedUser) {
            notify("Search for and select a person first.", "error");
            return;
        }

        setSaving(true);

        try {
            await api.post(`/projects/${activeProject.id}/members`, {
                userId: selectedUser.userId,
                role: memberForm.role === "Manager" ? 2 : 1,
            });

            const response = await api.get(
                `/projects/${activeProject.id}/members`,
            );

            setMembers(asArray<Member>(response.data));
            setMembersProjectId(activeProject.id);

            setModal(null);
            resetMemberSearch();
            notify("Team member added.");
        } catch (error) {
            notify(errorMessage(error), "error");
        } finally {
            setSaving(false);
        }
    }

    function resetMemberSearch() {
        setMemberSearch("");
        setAvailableUsers([]);
        setSelectedUser(null);
        setSearchingUsers(false);
        setMemberSearchError("");
    }

    function openModal(kind: Exclude<ModalType, null>) {
        if (kind === "task") {
            setTaskForm((current) => ({
                ...current,
                projectId: activeProject?.id || projects[0]?.id || "",
            }));
        }

        if (kind === "member") resetMemberSearch();

        setModal(kind);
    }

    function refreshWorkspace() {
        setLoading(true);
        setRefreshKey((value) => value + 1);
    }

    const navItems: { label: Page; icon: ReactNode; count?: number }[] = [
        { label: "Overview", icon: <LayoutDashboard size={18} /> },
        { label: "Projects", icon: <FolderKanban size={18} /> },
        { label: "My tasks", icon: <ListTodo size={18} /> },
        { label: "Team", icon: <Users size={18} /> },
        {
            label: "Notifications",
            icon: <Bell size={18} />,
            count: unreadCount,
        },
    ];

    const pageDescriptions: Record<Page, string> = {
        Overview: "A clear picture of what is moving and what needs attention.",
        Projects: "Bring initiatives, priorities, and next steps into focus.",
        "My tasks": "A practical view of the work currently on your plate.",
        Team: "The people collaborating on your selected project.",
        Notifications: "Recent updates from your workspace.",
        Settings: "Your account and current workspace information.",
    };

    if (!token) {
        return (
            <main className="auth-screen">
                <div className="auth-decoration auth-decoration-one" />
                <div className="auth-decoration auth-decoration-two" />

                <header className="auth-brand">
                    <span className="brand-symbol">
                        <Command size={21} />
                    </span>
                    <span className="brand-wordmark">projectflow</span>
                    <span className="brand-version">WORKSPACE</span>
                </header>

                <div className="auth-layout">
                    <section className="auth-story">
                        <span className="eyebrow">
                            <Sparkles size={15} /> A clearer way to work
                        </span>
                        <h1>
                            Good work deserves
                            <br />
                            <span>a better flow.</span>
                        </h1>
                        <p className="auth-lede">
                            Bring projects, people, and priorities into one calm, focused
                            workspace. Spend less time tracking work and more time moving it
                            forward.
                        </p>

                        <div className="auth-feature-list">
                            <div className="auth-feature">
                                <span className="feature-icon feature-violet">
                                    <FolderKanban size={18} />
                                </span>
                                <div>
                                    <strong>Projects with context</strong>
                                    <p>Keep the goal and the next step in view.</p>
                                </div>
                            </div>
                            <div className="auth-feature">
                                <span className="feature-icon feature-green">
                                    <Users size={18} />
                                </span>
                                <div>
                                    <strong>People in sync</strong>
                                    <p>Bring the right teammates into each project.</p>
                                </div>
                            </div>
                            <div className="auth-feature">
                                <span className="feature-icon feature-orange">
                                    <Zap size={18} />
                                </span>
                                <div>
                                    <strong>Progress you can see</strong>
                                    <p>Understand your workload at a glance.</p>
                                </div>
                            </div>
                        </div>

                        <div className="auth-proof">
                            <span className="proof-dot" />
                            Built for focused teams and meaningful progress
                        </div>
                    </section>

                    <section className="auth-card">
                        <div className="auth-card-top">
                            <span className="auth-card-icon">
                                <ShieldCheck size={19} />
                            </span>
                            <span className="auth-card-label">YOUR WORKSPACE</span>
                        </div>

                        <div className="auth-card-heading">
                            <h2>
                                {authMode === "login"
                                    ? "Welcome back"
                                    : "Create your account"}
                            </h2>
                            <p>
                                {authMode === "login"
                                    ? "Sign in to pick up where you left off."
                                    : "Set up your account and start organizing your work."}
                            </p>
                        </div>

                        <form className="form-stack" onSubmit={submitAuth}>
                            <label className="field">
                                <span>Workspace slug</span>
                                <input
                                    required
                                    autoComplete="organization"
                                    pattern="[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*"
                                    value={tenant}
                                    onChange={(event) => {
                                        setTenant(event.target.value);
                                        setTenantDraft(event.target.value);
                                    }}
                                    placeholder="globex"
                                />
                            </label>
                            {authMode === "register" && (
                                <>
                                    <label className="field">
                                        <span>Full name</span>
                                        <input
                                            required
                                            autoComplete="name"
                                            value={authForm.displayName}
                                            onChange={(event) =>
                                                setAuthForm((current) => ({
                                                    ...current,
                                                    displayName: event.target.value,
                                                }))
                                            }
                                            placeholder="Alex Morgan"
                                        />
                                    </label>
                                    <label className="field">
                                        <span>Username</span>
                                        <input
                                            required
                                            autoComplete="username"
                                            minLength={3}
                                            value={authForm.userName}
                                            onChange={(event) =>
                                                setAuthForm((current) => ({
                                                    ...current,
                                                    userName: event.target.value,
                                                }))
                                            }
                                            placeholder="alexmorgan"
                                        />
                                    </label>
                                </>
                            )}

                            <label className="field">
                                <span>Email address</span>
                                <input
                                    required
                                    type="email"
                                    autoComplete="email"
                                    value={authForm.email}
                                    onChange={(event) =>
                                        setAuthForm((current) => ({
                                            ...current,
                                            email: event.target.value,
                                        }))
                                    }
                                    placeholder="you@company.com"
                                />
                            </label>

                            <label className="field">
                                <span>Password</span>
                                <input
                                    required
                                    type="password"
                                    minLength={authMode === "register" ? 8 : undefined}
                                    autoComplete={
                                        authMode === "login"
                                            ? "current-password"
                                            : "new-password"
                                    }
                                    value={authForm.password}
                                    onChange={(event) =>
                                        setAuthForm((current) => ({
                                            ...current,
                                            password: event.target.value,
                                        }))
                                    }
                                    placeholder="Enter your password"
                                />
                            </label>

                            {authError && (
                                <p className="inline-error" role="alert">
                                    <AlertCircle size={16} />
                                    {authError}
                                </p>
                            )}

                            <button
                                className="button button-primary button-wide auth-submit"
                                disabled={authLoading}
                            >
                                {authLoading ? (
                                    <LoaderCircle size={17} className="spin" />
                                ) : (
                                    <ArrowRight size={17} />
                                )}
                                {authMode === "login" ? "Sign in" : "Create account"}
                            </button>
                        </form>

                        <p className="auth-switch">
                            {authMode === "login"
                                ? "New to ProjectFlow?"
                                : "Already have an account?"}
                            <button
                                type="button"
                                onClick={() => {
                                    setAuthMode((current) =>
                                        current === "login" ? "register" : "login",
                                    );
                                    setAuthError("");
                                }}
                            >
                                {authMode === "login" ? "Create an account" : "Sign in"}
                            </button>
                        </p>

                        <div className="auth-secure">
                            <ShieldCheck size={15} />
                            Your session is authenticated by your workspace API.
                        </div>
                    </section>
                </div>

                <footer className="auth-footer">
                    <span>© {new Date().getFullYear()} ProjectFlow</span>
                    <span>Designed for teams that value focus.</span>
                </footer>
            </main>
        );
    }

    return (
        <div className="app-shell">
            {mobileNavOpen && (
                <button
                    type="button"
                    className="mobile-scrim"
                    aria-label="Close navigation"
                    onClick={() => setMobileNavOpen(false)}
                />
            )}

            <aside className={`sidebar ${mobileNavOpen ? "sidebar-open" : ""}`}>
                <div className="sidebar-brand">
                    <span className="brand-symbol">
                        <Command size={20} />
                    </span>
                    <span className="brand-wordmark">projectflow</span>
                    <IconButton
                        label="Close navigation"
                        onClick={() => setMobileNavOpen(false)}
                    >
                        <X size={18} />
                    </IconButton>
                </div>

                <div className="workspace-switcher">
                    <span className="workspace-avatar">{initials(tenantLabel)}</span>
                    <span className="workspace-copy">
                        <strong>{tenantLabel}</strong>
                        <small>Current workspace</small>
                    </span>
                    <ChevronDown size={15} />
                </div>

                <p className="nav-section-label">WORKSPACE</p>
                <nav className="main-nav" aria-label="Main navigation">
                    {navItems.map((item) => (
                        <button
                            type="button"
                            key={item.label}
                            className={`nav-item ${page === item.label ? "nav-item-active" : ""}`}
                            onClick={() => {
                                setPage(item.label);
                                setMobileNavOpen(false);
                            }}
                        >
                            {item.icon}
                            <span>{item.label}</span>
                            {item.count ? <span className="nav-count">{item.count}</span> : null}
                        </button>
                    ))}
                </nav>

                <div className="sidebar-section-title">
                    <p className="nav-section-label">YOUR PROJECTS</p>
                    <IconButton
                        label="Create project"
                        onClick={() => openModal("project")}
                    >
                        <Plus size={16} />
                    </IconButton>
                </div>

                <div className="sidebar-project-list">
                    {projects.slice(0, 6).map((project, index) => (
                        <button
                            type="button"
                            key={project.id}
                            className={`sidebar-project ${activeProject?.id === project.id ? "sidebar-project-active" : ""}`}
                            onClick={() => {
                                setActiveProjectId(project.id);
                                setPage("Projects");
                                setMobileNavOpen(false);
                            }}
                        >
                            <span className={`project-color-dot project-color-${index % 4}`} />
                            <span>{project.name}</span>
                        </button>
                    ))}

                    {!projects.length && (
                        <p className="sidebar-empty">Your projects will appear here.</p>
                    )}

                    {projects.length > 6 && (
                        <button
                            type="button"
                            className="sidebar-project sidebar-project-muted"
                            onClick={() => setPage("Projects")}
                        >
                            View all projects <ArrowRight size={14} />
                        </button>
                    )}
                </div>

                <div className="sidebar-bottom">
                    <div className="sidebar-tip">
                        <span className="sidebar-tip-icon">
                            <CircleHelp size={17} />
                        </span>
                        <div>
                            <strong>Keep your flow</strong>
                            <p>One clear next step at a time.</p>
                        </div>
                    </div>

                    <button
                        type="button"
                        className={`nav-item ${page === "Settings" ? "nav-item-active" : ""}`}
                        onClick={() => {
                            setPage("Settings");
                            setMobileNavOpen(false);
                        }}
                    >
                        <Settings size={18} />
                        <span>Settings</span>
                    </button>

                    <div className="sidebar-user">
                        <Avatar name={displayName} />
                        <div className="sidebar-user-copy">
                            <strong>{displayName}</strong>
                            <small>{user?.email || "Signed in"}</small>
                        </div>
                        <IconButton label="Sign out" onClick={signOut}>
                            <LogOut size={16} />
                        </IconButton>
                    </div>
                </div>
            </aside>

            <main className="main-area">
                <header className="topbar">
                    <div className="topbar-leading">
                        <IconButton
                            label="Open navigation"
                            onClick={() => setMobileNavOpen(true)}
                        >
                            <Menu size={20} />
                        </IconButton>
                        <div className="breadcrumb">
                            <span>Workspace</span>
                            <span className="breadcrumb-slash">/</span>
                            <strong>{page}</strong>
                        </div>
                    </div>

                    <div className="topbar-actions">
                        <label className="global-search">
                            <Search size={17} />
                            <input
                                value={search}
                                onChange={(event) => setSearch(event.target.value)}
                                placeholder="Search projects and tasks"
                                aria-label="Search projects and tasks"
                            />
                            {search && (
                                <button
                                    type="button"
                                    className="search-clear"
                                    aria-label="Clear search"
                                    onClick={() => setSearch("")}
                                >
                                    <X size={14} />
                                </button>
                            )}
                            {!search && <kbd>⌘ K</kbd>}
                        </label>

                        <IconButton
                            label="Open notifications"
                            onClick={() => setPage("Notifications")}
                        >
                            <Bell size={19} />
                        </IconButton>

                        <button
                            type="button"
                            className="topbar-user"
                            onClick={() => setPage("Settings")}
                        >
                            <Avatar name={displayName} size="sm" />
                            <span>{displayName}</span>
                            <ChevronDown size={14} />
                        </button>
                    </div>
                </header>

                <div className="page-content">
                    <div className="page-heading">
                        <div>
                            <div className="eyebrow">
                                <span className="eyebrow-dot" />
                                YOUR WORKSPACE
                            </div>
                            <h1>
                                {page === "Overview"
                                    ? `Good to see you, ${displayName.split(" ")[0]}`
                                    : page}
                            </h1>
                            <p>{pageDescriptions[page]}</p>
                        </div>

                        <div className="heading-actions">
                            <button
                                type="button"
                                className="button button-secondary"
                                onClick={refreshWorkspace}
                                disabled={loading}
                            >
                                <RefreshCw size={16} className={loading ? "spin" : ""} />
                                Refresh
                            </button>

                            {(page === "Overview" || page === "Projects") && (
                                <button
                                    type="button"
                                    className="button button-primary"
                                    onClick={() => openModal("project")}
                                >
                                    <Plus size={17} />
                                    New project
                                </button>
                            )}

                            {page === "My tasks" && (
                                <button
                                    type="button"
                                    className="button button-primary"
                                    onClick={() => openModal("task")}
                                    disabled={!projects.length}
                                >
                                    <Plus size={17} />
                                    New task
                                </button>
                            )}

                            {page === "Team" && (
                                <button
                                    type="button"
                                    className="button button-primary"
                                    onClick={() => openModal("member")}
                                    disabled={!activeProject}
                                >
                                    <Plus size={17} />
                                    Add member
                                </button>
                            )}
                        </div>
                    </div>

                    {loading && (
                        <div className="loading-line" role="status">
                            <LoaderCircle size={16} className="spin" />
                            Syncing workspace data…
                        </div>
                    )}

                    {page === "Overview" && (
                        <>
                            <section className="hero-banner">
                                <div className="hero-copy">
                                    <span className="hero-eyebrow">
                                        <Sparkles size={14} /> YOUR MOMENTUM
                                    </span>
                                    <h2>
                                        Progress comes
                                        <br />
                                        from <span>clear priorities.</span>
                                    </h2>
                                    <p>
                                        Your work, projects, and people — organized into one
                                        workspace that helps you move forward.
                                    </p>
                                    <button
                                        type="button"
                                        className="button button-light"
                                        onClick={() => setPage("My tasks")}
                                    >
                                        Explore your tasks <ArrowRight size={16} />
                                    </button>
                                </div>

                                <div className="hero-art" aria-hidden="true">
                                    <div className="hero-ring hero-ring-one" />
                                    <div className="hero-ring hero-ring-two" />
                                    <div className="hero-center">
                                        <Command size={35} />
                                    </div>
                                    <div className="hero-float hero-float-top">
                                        <Check size={14} /> Stay focused
                                    </div>
                                    <div className="hero-float hero-float-bottom">
                                        <span /> Keep moving
                                    </div>
                                </div>
                            </section>

                            <div className="section-heading">
                                <div>
                                    <h2>Workspace at a glance</h2>
                                    <p>Live totals from the data available to this account.</p>
                                </div>
                                <span className="live-indicator">
                                    <span /> Workspace
                                </span>
                            </div>

                            <section className="stats-grid">
                                <StatCard
                                    label="Projects"
                                    value={projects.length}
                                    caption="Projects you can access"
                                    icon={<FolderKanban size={19} />}
                                    tone="tone-violet"
                                />
                                <StatCard
                                    label="Open tasks"
                                    value={openTasks.length}
                                    caption="Tasks not marked as done"
                                    icon={<ListTodo size={19} />}
                                    tone="tone-blue"
                                />
                                <StatCard
                                    label="In progress"
                                    value={inProgressTasks.length}
                                    caption="Tasks currently moving"
                                    icon={<Activity size={19} />}
                                    tone="tone-orange"
                                />
                                <StatCard
                                    label="Completed"
                                    value={completedTasks.length}
                                    caption="Tasks marked as done"
                                    icon={<Check size={19} />}
                                    tone="tone-green"
                                />
                            </section>

                            <div className="dashboard-grid">
                                <section className="panel">
                                    <div className="panel-heading">
                                        <div>
                                            <h2>Recent tasks</h2>
                                            <p>The latest tasks across your projects.</p>
                                        </div>
                                        <button
                                            type="button"
                                            className="text-button"
                                            onClick={() => setPage("My tasks")}
                                        >
                                            View all <ArrowRight size={15} />
                                        </button>
                                    </div>

                                    <TaskTable
                                        items={[...workItems]
                                            .sort((a, b) =>
                                                (b.createdAtUtc || "").localeCompare(a.createdAtUtc || ""),
                                            )
                                            .slice(0, 5)}
                                        projects={projects}
                                        onStatusChange={updateTaskStatus}
                                        onPriorityChange={updateTaskPriority}
                                        onOpen={(item) => void openTaskDetails(item)}
                                    />
                                </section>

                                <section className="panel activity-panel">
                                    <div className="panel-heading">
                                        <div>
                                            <h2>Project activity</h2>
                                            <p>
                                                {activeProject?.name || "Choose a project"} activity
                                            </p>
                                        </div>
                                        <span className="panel-heading-icon">
                                            <Activity size={17} />
                                        </span>
                                    </div>

                                    {activeProject && activitiesProjectId === activeProject.id &&
                                        activities.length ? (
                                        <div className="activity-list">
                                            {activities.slice(0, 6).map((item) => (
                                                <ActivityRow key={item.id} item={item} />
                                            ))}
                                        </div>
                                    ) : (
                                        <EmptyState
                                            icon={<Clock3 size={21} />}
                                            title="A little quiet here"
                                            description={
                                                activeProject
                                                    ? "Activity will appear here when the API returns project updates."
                                                    : "Create a project to start organizing your work."
                                            }
                                            action={
                                                !activeProject ? (
                                                    <button
                                                        type="button"
                                                        className="button button-primary button-small"
                                                        onClick={() => openModal("project")}
                                                    >
                                                        <Plus size={14} /> Create project
                                                    </button>
                                                ) : undefined
                                            }
                                        />
                                    )}
                                </section>
                            </div>

                            <section className="panel project-preview-panel">
                                <div className="panel-heading">
                                    <div>
                                        <h2>Your projects</h2>
                                        <p>A quick look at the initiatives in your workspace.</p>
                                    </div>
                                    <button
                                        type="button"
                                        className="text-button"
                                        onClick={() => setPage("Projects")}
                                    >
                                        All projects <ArrowRight size={15} />
                                    </button>
                                </div>

                                {projects.length ? (
                                    <div className="project-grid">
                                        {projects.slice(0, 3).map((project, index) => {
                                            const tasks = workItems.filter(
                                                (item) => item.projectId === project.id,
                                            );
                                            const done = tasks.filter(isDone).length;
                                            const progress = tasks.length
                                                ? Math.round((done / tasks.length) * 100)
                                                : 0;

                                            return (
                                                <button
                                                    type="button"
                                                    key={project.id}
                                                    className="project-card"
                                                    onClick={() => {
                                                        setActiveProjectId(project.id);
                                                        setPage("Projects");
                                                    }}
                                                >
                                                    <div className="project-card-top">
                                                        <span className={`project-icon project-icon-${index % 4}`}>
                                                            <FolderKanban size={19} />
                                                        </span>
                                                        <span className="project-card-arrow">
                                                            <ArrowRight size={16} />
                                                        </span>
                                                    </div>
                                                    <h3>{project.name}</h3>
                                                    <p>{project.description || "No description added yet."}</p>
                                                    <div className="project-progress-label">
                                                        <span>{tasks.length} tasks</span>
                                                        <strong>{progress}%</strong>
                                                    </div>
                                                    <div className="progress-track">
                                                        <span style={{ width: `${progress}%` }} />
                                                    </div>
                                                </button>
                                            );
                                        })}
                                    </div>
                                ) : (
                                    <EmptyState
                                        icon={<FolderKanban size={22} />}
                                        title="Start with a project"
                                        description="Create your first project to give your team's work a home."
                                        action={
                                            <button
                                                type="button"
                                                className="button button-primary button-small"
                                                onClick={() => openModal("project")}
                                            >
                                                <Plus size={14} /> Create project
                                            </button>
                                        }
                                    />
                                )}
                            </section>
                        </>
                    )}

                    {page === "Projects" && (
                        <section className="panel full-panel">
                            <div className="panel-heading">
                                <div>
                                    <h2>Project directory</h2>
                                    <p>
                                        {filteredProjects.length} of {projects.length} projects
                                    </p>
                                </div>
                                <span className="panel-heading-icon">
                                    <FolderKanban size={18} />
                                </span>
                            </div>

                            {filteredProjects.length ? (
                                <div className="project-grid project-grid-large">
                                    {filteredProjects.map((project, index) => {
                                        const tasks = workItems.filter(
                                            (item) => item.projectId === project.id,
                                        );
                                        const done = tasks.filter(isDone).length;
                                        const progress = tasks.length
                                            ? Math.round((done / tasks.length) * 100)
                                            : 0;
                                        const selected = activeProject?.id === project.id;

                                        return (
                                            <button
                                                type="button"
                                                key={project.id}
                                                className={`project-card ${selected ? "project-card-selected" : ""}`}
                                                onClick={() => setActiveProjectId(project.id)}
                                            >
                                                <div className="project-card-top">
                                                    <span className={`project-icon project-icon-${index % 4}`}>
                                                        <FolderKanban size={19} />
                                                    </span>
                                                    {selected && <span className="selected-label">Selected</span>}
                                                </div>
                                                <h3>{project.name}</h3>
                                                <p>{project.description || "No description added yet."}</p>
                                                <div className="project-card-meta">
                                                    <span>
                                                        <ListTodo size={14} /> {tasks.length} tasks
                                                    </span>
                                                    <span>
                                                        <Check size={14} /> {done} completed
                                                    </span>
                                                </div>
                                                <div className="project-progress-label">
                                                    <span>Completion</span>
                                                    <strong>{progress}%</strong>
                                                </div>
                                                <div className="progress-track">
                                                    <span style={{ width: `${progress}%` }} />
                                                </div>
                                                <span className="project-card-footer">
                                                    {selected ? "Currently selected" : "Select project"}
                                                    <ArrowRight size={15} />
                                                </span>
                                            </button>
                                        );
                                    })}
                                </div>
                            ) : (
                                <EmptyState
                                    icon={<FolderKanban size={22} />}
                                    title={search ? "No matching projects" : "Your first project starts here"}
                                    description={
                                        search
                                            ? "Try a different search term."
                                            : "Create a project to organize the work ahead."
                                    }
                                    action={
                                        !search ? (
                                            <button
                                                type="button"
                                                className="button button-primary button-small"
                                                onClick={() => openModal("project")}
                                            >
                                                <Plus size={14} /> New project
                                            </button>
                                        ) : undefined
                                    }
                                />
                            )}

                            {activeProject && (
                                <div className="selected-project-summary">
                                    <div>
                                        <span className="eyebrow">SELECTED PROJECT</span>
                                        <h3>{activeProject.name}</h3>
                                        <p>{activeProject.description || "No description added yet."}</p>
                                    </div>
                                    <div className="selected-project-metrics">
                                        <div>
                                            <strong>{projectTasks.length}</strong>
                                            <span>Tasks</span>
                                        </div>
                                        <div>
                                            <strong>{projectCompleted}</strong>
                                            <span>Completed</span>
                                        </div>
                                        <div>
                                            <strong>{membersProjectId === activeProject.id ? members.length : "—"}</strong>
                                            <span>Members</span>
                                        </div>
                                    </div>
                                </div>
                            )}
                        </section>
                    )}

                    {page === "My tasks" && (
                        <section className="panel full-panel">
                            <div className="panel-heading">
                                <div>
                                    <h2>Task list</h2>
                                    <p>{filteredTasks.length} tasks match your current filters.</p>
                                </div>
                                <span className="panel-heading-icon">
                                    <ListTodo size={18} />
                                </span>
                            </div>

                            <div className="task-filters">
                                <label>
                                    <span>Status</span>
                                    <select
                                        value={taskStatusFilter}
                                        onChange={(event) => setTaskStatusFilter(event.target.value)}
                                    >
                                        <option>All</option>
                                        <option>To do</option>
                                        <option>In progress</option>
                                        <option>In review</option>
                                        <option>Completed</option>
                                    </select>
                                </label>
                                <label>
                                    <span>Priority</span>
                                    <select
                                        value={taskPriorityFilter}
                                        onChange={(event) => setTaskPriorityFilter(event.target.value)}
                                    >
                                        <option>All</option>
                                        <option>Low</option>
                                        <option>Medium</option>
                                        <option>High</option>
                                        <option>Critical</option>
                                    </select>
                                </label>
                                <button
                                    type="button"
                                    className="button button-secondary button-small"
                                    onClick={() => {
                                        setSearch("");
                                        setTaskStatusFilter("All");
                                        setTaskPriorityFilter("All");
                                    }}
                                >
                                    Clear filters
                                </button>
                            </div>

                            <TaskTable
                                items={filteredTasks}
                                projects={projects}
                                onStatusChange={updateTaskStatus}
                                onPriorityChange={updateTaskPriority}
                                onOpen={(item) => void openTaskDetails(item)}
                            />
                        </section>
                    )}

                    {page === "Team" && (
                        <div className="team-layout">
                            <section className="panel full-panel">
                                <div className="panel-heading">
                                    <div>
                                        <h2>{activeProject?.name || "Project team"}</h2>
                                        <p>People who have access to this project.</p>
                                    </div>
                                    <span className="count-pill">
                                        <Users size={14} />
                                        {membersProjectId === activeProject?.id ? members.length : "—"} members
                                    </span>
                                </div>

                                {!activeProject ? (
                                    <EmptyState
                                        icon={<FolderKanban size={22} />}
                                        title="Create a project first"
                                        description="Project members are managed within a selected project."
                                        action={
                                            <button
                                                type="button"
                                                className="button button-primary button-small"
                                                onClick={() => openModal("project")}
                                            >
                                                <Plus size={14} /> Create project
                                            </button>
                                        }
                                    />
                                ) : membersProjectId !== activeProject.id ? (
                                    <div className="loading-placeholder">
                                        <LoaderCircle size={18} className="spin" />
                                        Loading project members…
                                    </div>
                                ) : members.length ? (
                                    <div className="member-list">
                                        {members.map((member) => {
                                            const name =
                                                member.displayName ||
                                                member.userName ||
                                                member.email ||
                                                "Workspace member";
                                            const manager = roleLabel(member.role) === "Manager";

                                            return (
                                                <article className="member-row" key={member.id}>
                                                    <Avatar name={name} size="lg" />
                                                    <div className="member-info">
                                                        <strong>{name}</strong>
                                                        <small>
                                                            {member.email ||
                                                                (member.userName ? `@${member.userName}` : "Project member")}
                                                        </small>
                                                    </div>
                                                    <span className={`role-pill ${manager ? "role-manager" : ""}`}>
                                                        {roleLabel(member.role)}
                                                    </span>
                                                    <select
                                                        className="field-select"
                                                        aria-label={`Change ${name}'s project role`}
                                                        value={typeof member.role === "number" ? member.role : manager ? 2 : 1}
                                                        onChange={(event) => changeMemberRole(member, Number(event.target.value))}
                                                    >
                                                        <option value={1}>Member</option>
                                                        <option value={2}>Manager</option>
                                                    </select>
                                                    <button
                                                        type="button"
                                                        className="text-button"
                                                        onClick={() => void removeMember(member)}
                                                    >
                                                        Remove
                                                    </button>
                                                </article>
                                            );
                                        })}
                                    </div>
                                ) : (
                                    <EmptyState
                                        icon={<Users size={22} />}
                                        title="Build your project team"
                                        description="Search for an existing user and add them to this project."
                                        action={
                                            <button
                                                type="button"
                                                className="button button-primary button-small"
                                                onClick={() => openModal("member")}
                                            >
                                                <Plus size={14} /> Add member
                                            </button>
                                        }
                                    />
                                )}
                            </section>

                            <aside className="panel team-aside">
                                <span className="team-aside-icon">
                                    <Users size={21} />
                                </span>
                                <h3>Better work happens together.</h3>
                                <p>
                                    Keep the right people close to the work and make project
                                    ownership easier to understand.
                                </p>
                                <div className="team-aside-stat">
                                    <span>Selected project</span>
                                    <strong>{activeProject?.name || "None selected"}</strong>
                                </div>
                                <div className="team-aside-stat">
                                    <span>Team size</span>
                                    <strong>
                                        {membersProjectId === activeProject?.id ? members.length : "—"}
                                    </strong>
                                </div>
                                <button
                                    type="button"
                                    className="button button-secondary button-wide"
                                    disabled={!activeProject}
                                    onClick={() => openModal("member")}
                                >
                                    <Plus size={15} /> Add teammate
                                </button>
                            </aside>
                        </div>
                    )}

                    {page === "Notifications" && (
                        <section className="panel full-panel">
                            <div className="panel-heading">
                                <div>
                                    <h2>Notifications</h2>
                                    <p>
                                        {unreadCount
                                            ? `${unreadCount} unread notification${unreadCount === 1 ? "" : "s"}`
                                            : "You're all caught up."}
                                    </p>
                                </div>
                                <span className="panel-heading-icon">
                                    <Bell size={18} />
                                </span>
                            </div>

                            {notifications.length ? (
                                <div className="notification-list">
                                    {notifications.map((item) => {
                                        const read = item.isRead ?? item.read ?? false;

                                        return (
                                            <article
                                                key={item.id}
                                                className={`notification-row ${read ? "notification-read" : ""}`}
                                            >
                                                <span className="notification-icon">
                                                    <Bell size={17} />
                                                </span>
                                                <div className="notification-copy">
                                                    <strong>{item.title || "Workspace update"}</strong>
                                                    <p>
                                                        {item.message || "There is a new update in your workspace."}
                                                    </p>
                                                    <small>{relativeTime(item.createdAtUtc)}</small>
                                                </div>
                                                {!read && (
                                                    <button
                                                        type="button"
                                                        className="text-button"
                                                        onClick={() => void markNotificationRead(item)}
                                                    >
                                                        Mark read
                                                    </button>
                                                )}
                                            </article>
                                        );
                                    })}
                                </div>
                            ) : (
                                <EmptyState
                                    icon={<Bell size={22} />}
                                    title="Nothing new right now"
                                    description="When the notifications API returns updates, they will appear here."
                                />
                            )}
                        </section>
                    )}

                    {page === "Settings" && (
                        <div className="settings-grid">
                            <section className="panel settings-panel">
                                <div className="panel-heading">
                                    <div>
                                        <h2>Your profile</h2>
                                        <p>Information associated with this session.</p>
                                    </div>
                                    <span className="panel-heading-icon">
                                        <Users size={18} />
                                    </span>
                                </div>

                                <div className="profile-preview">
                                    <Avatar name={displayName} size="lg" />
                                    <div>
                                        <strong>{displayName}</strong>
                                        <small>{user?.email || "Email unavailable"}</small>
                                    </div>
                                </div>
                                <div className="settings-row">
                                    <span>Display name</span>
                                    <strong>{displayName}</strong>
                                </div>
                                <div className="settings-row">
                                    <span>Email address</span>
                                    <strong>{user?.email || "Not available"}</strong>
                                </div>
                                <div className="settings-row">
                                    <span>Authentication</span>
                                    <span className="connection-pill">
                                        <ShieldCheck size={14} /> Signed in
                                    </span>
                                </div>
                            </section>

                            <section className="panel settings-panel">
                                <div className="panel-heading">
                                    <div>
                                        <h2>Workspace</h2>
                                        <p>Your current tenant context.</p>
                                    </div>
                                    <span className="panel-heading-icon">
                                        <ShieldCheck size={18} />
                                    </span>
                                </div>

                                <div className="workspace-settings-card">
                                    <span className="workspace-avatar workspace-avatar-large">
                                        {initials(tenantLabel)}
                                    </span>
                                    <div>
                                        <strong>{tenantLabel}</strong>
                                        <small>Active workspace</small>
                                    </div>
                                    <span className="connection-pill">
                                        <span className="connection-dot" /> Active
                                    </span>
                                </div>

                                <form className="modal-form" onSubmit={saveTenant}>
                                    <label className="field">
                                        <span>Workspace slug</span>
                                        <input
                                            required
                                            pattern="[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*"
                                            value={tenantDraft}
                                            onChange={(event) => setTenantDraft(event.target.value)}
                                        />
                                    </label>
                                    <button className="button button-secondary button-wide">
                                        Switch workspace
                                    </button>
                                </form>

                                <div className="settings-row">
                                    <span>Projects accessible</span>
                                    <strong>{projects.length}</strong>
                                </div>
                                <div className="settings-row">
                                    <span>Tasks loaded</span>
                                    <strong>{workItems.length}</strong>
                                </div>

                                <div className="settings-note">
                                    <ShieldCheck size={16} />
                                    <span>
                                        Your requests use the configured tenant header and current
                                        authentication token.
                                    </span>
                                </div>

                                <button
                                    type="button"
                                    className="button button-secondary button-wide signout-button"
                                    onClick={signOut}
                                >
                                    <LogOut size={16} /> Sign out
                                </button>
                            </section>
                        </div>
                    )}

                    <footer className="page-footer">
                        <span>Make room for meaningful work.</span>
                        <span>
                            ProjectFlow <span className="footer-separator">/</span>{" "}
                            {new Date().getFullYear()}
                        </span>
                    </footer>
                </div>
            </main>

            {notice && (
                <div
                    className={`toast toast-${notice.type}`}
                    role={notice.type === "error" ? "alert" : "status"}
                >
                    <span className="toast-icon">
                        {notice.type === "success" ? (
                            <Check size={16} />
                        ) : (
                            <AlertCircle size={16} />
                        )}
                    </span>
                    <span>{notice.message}</span>
                    <IconButton label="Dismiss notification" onClick={() => setNotice(null)}>
                        <X size={15} />
                    </IconButton>
                </div>
            )}

            {modal === "project" && (
                <Modal
                    title="Create a project"
                    description="Give the next initiative a clear starting point."
                    onClose={() => setModal(null)}
                >
                    <form className="modal-form" onSubmit={createProject}>
                        <label className="field">
                            <span>Project name</span>
                            <input
                                autoFocus
                                required
                                maxLength={120}
                                value={projectForm.name}
                                onChange={(event) =>
                                    setProjectForm((current) => ({
                                        ...current,
                                        name: event.target.value,
                                    }))
                                }
                                placeholder="e.g. Customer portal"
                            />
                        </label>

                        <label className="field">
                            <span>
                                Description <em>Optional</em>
                            </span>
                            <textarea
                                rows={4}
                                maxLength={500}
                                value={projectForm.description}
                                onChange={(event) =>
                                    setProjectForm((current) => ({
                                        ...current,
                                        description: event.target.value,
                                    }))
                                }
                                placeholder="What are you aiming to accomplish?"
                            />
                        </label>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="button button-secondary"
                                onClick={() => setModal(null)}
                            >
                                Cancel
                            </button>
                            <button
                                className="button button-primary"
                                disabled={saving || !projectForm.name.trim()}
                            >
                                {saving ? (
                                    <LoaderCircle size={16} className="spin" />
                                ) : (
                                    <Plus size={16} />
                                )}
                                Create project
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {modal === "task" && (
                <Modal
                    title="Create a task"
                    description="Turn the next step into something actionable."
                    onClose={() => setModal(null)}
                >
                    <form className="modal-form" onSubmit={createTask}>
                        <label className="field">
                            <span>Task title</span>
                            <input
                                autoFocus
                                required
                                maxLength={160}
                                value={taskForm.title}
                                onChange={(event) =>
                                    setTaskForm((current) => ({
                                        ...current,
                                        title: event.target.value,
                                    }))
                                }
                                placeholder="What needs to get done?"
                            />
                        </label>

                        <label className="field">
                            <span>
                                Description <em>Optional</em>
                            </span>
                            <textarea
                                rows={3}
                                value={taskForm.description}
                                onChange={(event) =>
                                    setTaskForm((current) => ({
                                        ...current,
                                        description: event.target.value,
                                    }))
                                }
                                placeholder="Add useful context for the team."
                            />
                        </label>

                        <div className="form-grid-two">
                            <label className="field">
                                <span>Project</span>
                                <select
                                    required
                                    value={taskForm.projectId}
                                    onChange={(event) =>
                                        setTaskForm((current) => ({
                                            ...current,
                                            projectId: event.target.value,
                                        }))
                                    }
                                >
                                    <option value="">Select a project</option>
                                    {projects.map((project) => (
                                        <option key={project.id} value={project.id}>
                                            {project.name}
                                        </option>
                                    ))}
                                </select>
                            </label>

                            <label className="field">
                                <span>Priority</span>
                                <select
                                    value={taskForm.priority}
                                    onChange={(event) =>
                                        setTaskForm((current) => ({
                                            ...current,
                                            priority: event.target.value,
                                        }))
                                    }
                                >
                                    <option>Low</option>
                                    <option>Medium</option>
                                    <option>High</option>
                                    <option>Critical</option>
                                </select>
                            </label>
                        </div>

                        <label className="field">
                            <span>
                                Due date <em>Optional</em>
                            </span>
                            <input
                                type="date"
                                value={taskForm.dueDateUtc}
                                onChange={(event) =>
                                    setTaskForm((current) => ({
                                        ...current,
                                        dueDateUtc: event.target.value,
                                    }))
                                }
                            />
                        </label>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="button button-secondary"
                                onClick={() => setModal(null)}
                            >
                                Cancel
                            </button>
                            <button
                                className="button button-primary"
                                disabled={
                                    saving || !taskForm.title.trim() || !taskForm.projectId
                                }
                            >
                                {saving ? (
                                    <LoaderCircle size={16} className="spin" />
                                ) : (
                                    <Plus size={16} />
                                )}
                                Create task
                            </button>
                        </div>
                    </form>
                </Modal>
            )}

            {modal === "taskDetails" && selectedTask && (
                <Modal
                    title="Task details"
                    description={projects.find((project) => project.id === selectedTask.projectId)?.name || "Project task"}
                    onClose={() => setModal(null)}
                >
                    {loadingTaskDetails ? (
                        <div className="loading-placeholder">
                            <LoaderCircle size={18} className="spin" /> Loading task details…
                        </div>
                    ) : (
                        <>
                            <form className="modal-form" onSubmit={saveTaskDetails}>
                                <label className="field">
                                    <span>Task title</span>
                                    <input
                                        required
                                        maxLength={160}
                                        value={selectedTask.title}
                                        onChange={(event) => setSelectedTask((current) => current
                                            ? { ...current, title: event.target.value }
                                            : current)}
                                    />
                                </label>
                                <label className="field">
                                    <span>Description</span>
                                    <textarea
                                        rows={3}
                                        value={selectedTask.description || ""}
                                        onChange={(event) => setSelectedTask((current) => current
                                            ? { ...current, description: event.target.value }
                                            : current)}
                                    />
                                </label>
                                <div className="form-grid-two">
                                    <label className="field">
                                        <span>Status</span>
                                        <select
                                            value={typeof selectedTask.status === "number"
                                                ? selectedTask.status
                                                : statusValue(statusLabel(selectedTask.status))}
                                            onChange={(event) => void updateTaskStatus(selectedTask, Number(event.target.value))}
                                        >
                                            <option value={1}>To do</option>
                                            <option value={2}>In progress</option>
                                            <option value={3}>In review</option>
                                            <option value={4}>Completed</option>
                                        </select>
                                    </label>
                                    <label className="field">
                                        <span>Priority</span>
                                        <select
                                            value={typeof selectedTask.priority === "number"
                                                ? selectedTask.priority
                                                : priorityValue(priorityLabel(selectedTask.priority))}
                                            onChange={(event) => void updateTaskPriority(selectedTask, Number(event.target.value))}
                                        >
                                            <option value={1}>Low</option>
                                            <option value={2}>Medium</option>
                                            <option value={3}>High</option>
                                            <option value={4}>Critical</option>
                                        </select>
                                    </label>
                                </div>
                                <label className="field">
                                    <span>Due date</span>
                                    <input
                                        type="date"
                                        value={(selectedTask.dueDateUtc || "").slice(0, 10)}
                                        onChange={(event) => setSelectedTask((current) => current
                                            ? { ...current, dueDateUtc: event.target.value || null }
                                            : current)}
                                    />
                                </label>
                                <div className="modal-actions">
                                    <button
                                        type="button"
                                        className="button button-secondary"
                                        onClick={() => void deleteTask()}
                                    >
                                        Delete task
                                    </button>
                                    <button className="button button-primary" disabled={saving || !selectedTask.title.trim()}>
                                        {saving ? <LoaderCircle size={16} className="spin" /> : <Check size={16} />}
                                        Save changes
                                    </button>
                                </div>
                            </form>

                            <section className="task-detail-section">
                                <div className="panel-heading">
                                    <div>
                                        <h3>Assignees</h3>
                                        <p>People responsible for this task.</p>
                                    </div>
                                </div>
                                {taskAssignments.length ? (
                                    <div className="member-list">
                                        {taskAssignments.map((assignment) => {
                                            const assignedMember = members.find((member) => member.userId === assignment.userId);
                                            const assignedName = assignedMember?.displayName || assignedMember?.userName || assignedMember?.email || "Project member";
                                            return (
                                                <div className="member-row" key={assignment.id}>
                                                    <Avatar name={assignedName} />
                                                    <div className="member-info"><strong>{assignedName}</strong></div>
                                                    {canManageSelectedTask && (
                                                        <button type="button" className="text-button" onClick={() => void removeTaskAssignment(assignment)}>
                                                            Remove
                                                        </button>
                                                    )}
                                                </div>
                                            );
                                        })}
                                    </div>
                                ) : <p className="form-help">No one is assigned yet.</p>}
                                {canManageSelectedTask && (
                                    <form className="form-grid-two" onSubmit={assignTask}>
                                        <label className="field">
                                            <span>Assign a project member</span>
                                            <select
                                                value={assignmentUserId}
                                                onChange={(event) => setAssignmentUserId(event.target.value)}
                                            >
                                                <option value="">Choose a teammate</option>
                                                {members.filter((member) =>
                                                    member.userId && !taskAssignments.some((assignment) => assignment.userId === member.userId),
                                                ).map((member) => (
                                                    <option key={member.userId} value={member.userId}>
                                                        {member.displayName || member.userName || member.email || "Project member"}
                                                    </option>
                                                ))}
                                            </select>
                                        </label>
                                        <button className="button button-secondary" disabled={!assignmentUserId}>
                                            Assign
                                        </button>
                                    </form>
                                )}
                            </section>

                            <section className="task-detail-section">
                                <div className="panel-heading">
                                    <div>
                                        <h3>Comments</h3>
                                        <p>Keep the conversation attached to the work.</p>
                                    </div>
                                </div>
                                {taskComments.length ? (
                                    <div className="activity-list">
                                        {taskComments.map((comment) => {
                                            const author = members.find((member) => member.userId === comment.authorUserId);
                                            return (
                                                <article className="activity-row" key={comment.id}>
                                                    <Avatar name={author?.displayName || author?.userName || "Team"} size="sm" />
                                                    <div className="activity-copy">
                                                        <p>{comment.content}</p>
                                                        <small>{relativeTime(comment.createdAtUtc)}</small>
                                                    </div>
                                                </article>
                                            );
                                        })}
                                    </div>
                                ) : <p className="form-help">No comments yet.</p>}
                                <form className="modal-form" onSubmit={addTaskComment}>
                                    <label className="field">
                                        <span>Add a comment</span>
                                        <textarea
                                            required
                                            maxLength={2000}
                                            rows={3}
                                            value={commentDraft}
                                            onChange={(event) => setCommentDraft(event.target.value)}
                                            placeholder="Share a useful update…"
                                        />
                                    </label>
                                    <button className="button button-primary button-small" disabled={!commentDraft.trim()}>
                                        Add comment
                                    </button>
                                </form>
                            </section>
                        </>
                    )}
                </Modal>
            )}

            {modal === "member" && (
                <Modal
                    title="Add a team member"
                    description={`Search for an existing user to add to ${activeProject?.name || "your project"}.`}
                    onClose={() => {
                        setModal(null);
                        resetMemberSearch();
                    }}
                >
                    <form className="modal-form" onSubmit={addMember}>
                        <label className="field">
                            <span>Find a person</span>
                            <input
                                autoFocus
                                type="search"
                                autoComplete="off"
                                minLength={2}
                                value={memberSearch}
                                onChange={(event) => {
                                    setMemberSearch(event.target.value);
                                    setSelectedUser(null);
                                    setAvailableUsers([]);
                                    setMemberSearchError("");
                                    setSearchingUsers(false);
                                }}
                                placeholder="Name, username, or email"
                            />
                        </label>

                        {memberSearch.trim().length === 1 && (
                            <p className="form-help">Enter at least 2 characters to search.</p>
                        )}

                        {searchingUsers && (
                            <div className="loading-placeholder">
                                <LoaderCircle size={16} className="spin" /> Searching users…
                            </div>
                        )}

                        {memberSearchError && (
                            <p className="inline-error" role="alert">
                                <AlertCircle size={15} /> {memberSearchError}
                            </p>
                        )}

                        {!searchingUsers &&
                            !memberSearchError &&
                            memberSearch.trim().length >= 2 &&
                            !selectedUser &&
                            availableUsers.length === 0 && (
                                <p className="form-help">
                                    No available users found. Try a different search.
                                </p>
                            )}

                        {!selectedUser && availableUsers.length > 0 && (
                            <div className="user-search-results" role="listbox">
                                {availableUsers.map((person) => {
                                    const name =
                                        person.displayName ||
                                        person.userName ||
                                        person.email ||
                                        "Workspace member";

                                    return (
                                        <button
                                            type="button"
                                            key={person.userId}
                                            className="user-search-result"
                                            onClick={() => {
                                                setSelectedUser(person);
                                                setAvailableUsers([]);
                                                setMemberSearch(name);
                                                setMemberSearchError("");
                                            }}
                                        >
                                            <Avatar name={name} />
                                            <span>
                                                <strong>{name}</strong>
                                                <small>
                                                    {person.email ||
                                                        (person.userName ? `@${person.userName}` : "Existing user")}
                                                </small>
                                            </span>
                                            <Plus size={16} />
                                        </button>
                                    );
                                })}
                            </div>
                        )}

                        {selectedUser && (
                            <div className="selected-user">
                                <Avatar
                                    name={
                                        selectedUser.displayName ||
                                        selectedUser.userName ||
                                        selectedUser.email
                                    }
                                />
                                <div>
                                    <strong>
                                        {selectedUser.displayName ||
                                            selectedUser.userName ||
                                            selectedUser.email}
                                    </strong>
                                    <small>{selectedUser.email || selectedUser.userName}</small>
                                </div>
                                <IconButton
                                    label="Choose another user"
                                    onClick={() => {
                                        setSelectedUser(null);
                                        setMemberSearch("");
                                        setAvailableUsers([]);
                                    }}
                                >
                                    <X size={16} />
                                </IconButton>
                            </div>
                        )}

                        <label className="field">
                            <span>Project role</span>
                            <select
                                value={memberForm.role}
                                onChange={(event) =>
                                    setMemberForm({ role: event.target.value })
                                }
                            >
                                <option value="Member">Member</option>
                                <option value="Manager">Manager</option>
                            </select>
                        </label>

                        <p className="form-help">
                            <ShieldCheck size={15} />
                            Only users returned by the project user-search API can be added.
                        </p>

                        <div className="modal-actions">
                            <button
                                type="button"
                                className="button button-secondary"
                                onClick={() => {
                                    setModal(null);
                                    resetMemberSearch();
                                }}
                            >
                                Cancel
                            </button>
                            <button
                                className="button button-primary"
                                disabled={saving || !activeProject || !selectedUser}
                            >
                                {saving ? (
                                    <LoaderCircle size={16} className="spin" />
                                ) : (
                                    <Plus size={16} />
                                )}
                                Add member
                            </button>
                        </div>
                    </form>
                </Modal>
            )}
        </div>
    );
}