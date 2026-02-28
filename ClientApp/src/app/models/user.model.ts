// ממשק משתמש - תואם ל-UserDTO של C#
export interface User {
  userId: number;
  userName: string;  
  firstName: string;
  lastName: string;
  password?: string;
  address?: string;
  phone?: string;
  isAdmin: boolean;
}

// ממשק להתחברות - תואם ל-UserLoginDTO של C#
export interface LoginRequest {
  userName: string;   // אימייל
  password: string;
}

// ממשק להרשמה - תואם ל-UserDTO של C#
export interface RegisterRequest {
  userId: number;
  userName: string;   // אימייל
  firstName: string;
  lastName: string;
  password: string;
  address: string;
  phone: string;
  isAdmin: boolean;
}
