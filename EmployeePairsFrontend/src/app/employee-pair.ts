export interface CommonProject {
  employeeId1: number;
  employeeId2: number;
  projectId: number;
  daysWorked: number;
}

export interface EmployeePair {
  employeeId1: number;
  employeeId2: number;
  totalDaysWorked: number;
  commonProjects: CommonProject[];
}
