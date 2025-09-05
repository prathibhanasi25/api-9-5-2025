const API_URL = "https://localhost:7233/api";

import axios from "axios";
console.log("Process Variable",process.env.REACT_APP_API_URL)

export const getImageBaseUrl = (profilepic) => {
    return `${API_URL}/Users/photo/${profilepic}`;
}

export async function loginAPI(data) {
  const res = await fetch(`${API_URL}/Users/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
 return await res.json();
}

export async function register(data) {
  const res = await fetch(`${API_URL}/Users/Register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
   const response = await res.json();
   return { status: response.status, message: response.message };
}

export async function SaveMember(data) {
  console.log("Call from SaveMember")
  const res = await fetch(`${API_URL}/Users/SaveMember`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
   const response = await res.json();
   console.log("member result",response);
   return { status: response.status, message: response.message,memberID: response.memberID};
}


export async function GetMemberInsuranceByMemberID(data) 
{
 try {
 const res = await fetch(`${API_URL}/Users/GetMemberInsuranceByMemberID?MemberID=${data}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
   if (!res.ok) {
     console.error(`Error: ${res.status} ${res.statusText}`);
      return null;
    }
  return await res.json();
   } catch (error) {
    console.error('Fetch error:', error);
    return null;
  }
}

export async function GetMemberInsuranceById(data) 
{

 const res = await fetch(`${API_URL}/Users/GetMemberInsuranceById?MemInsID=${data}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

export async function DeleteMemeberInsurance(deleteMemInsId, memberId) {
    try {
  console.log("data",deleteMemInsId, memberId);
  console.log("API URL:", `${API_URL}/Users/DeleteMemberInsurance?MemberInsuranceId=${deleteMemInsId}&memberId=${memberId}`);
  const res = await fetch(`${API_URL}/Users/DeleteMemberInsurance?MemberInsuranceId=${deleteMemInsId}&memberId=${memberId}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },    
  });
  return await res.json();
  } catch (err) {   
    console.error("Error in Delete Memeber Insurance API:", err);
    return { message: "Error in Delete Memeber Insurance API:" };
  } finally {

  } 
}

export async function ChangePasswordAPI(data) {
  const res = await fetch(`${API_URL}/Users/ChangePassword`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return await res.json();
  
}

export async function forgotPassword(data) {
    try {
  console.log("Forgot Password API called with data:", data.email);
  console.log("API URL:", `${API_URL}/Users/ForgotPassword?email=${data.email}`);
  const res = await fetch(`${API_URL}/Users/ForgotPassword?email=${data.email}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },    
  });
  return await res.json();
  } catch (err) {   
    console.error("Error in forgotPassword API:", err);
    return { message: "Failed to send reset password email. Please try again." };
  } finally {

  } 
}

export async function VerifyTokenAPI(data) {
  try {
    console.log(`${API_URL}/Users/${data.IsMobile=="1"? "Verify-Mobile":"Verify-Email"}`);
    const res = await fetch(`${API_URL}/Users/${data.IsMobile=="1"? "Verify-Mobile":"Verify-Email"}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  
    const response = await res.json();
      if (!res.ok) {
      return { status: "Error", message: "Token is not verified successfully. Please try again." };
    }else
    {    
    return { status: response.status, message: response.message };
    }
  } catch (err) {   
       return { status: "Error", message: "Token is not verified successfully. Please try again." };
  } finally {

  }  
}

export async function GetRelationshipType() 
{
 const res = await fetch(`${API_URL}/Users/GetCodeType?codeType=Relationship`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

export async function GetBenifitType() 
{
 const res = await fetch(`${API_URL}/Users/GetCodeType?codeType=BenefitType`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

/*export async function GetUserRole() 
{
 const res = await fetch(`${API_URL}/Users/GetCodeType?codeType=Role`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}*/

export async function GetUserRole() {
  const res = await fetch(`${API_URL}/Users/GetCodeType?codeType=Role`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(`Failed to fetch roles: ${errorText}`);
  }

  return await res.json(); // ✅ Will not fail anymore with your backend fix
}



export async function GetPolicyType() 
{
 const res = await fetch(`${API_URL}/Users/GetCodeType?codeType=PolicyType`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}



export async function GetDistinctState() 
{
 const res = await fetch(`${API_URL}/Users/GetDistinctState`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

export async function GetDistinctZipbyState(data) 
{

 const res = await fetch(`${API_URL}/Users/GetDistinctZipbyState?State=${data}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

export async function GetZipDatabyCode(data) 
{

 const res = await fetch(`${API_URL}/Users/GetZipDatabyCode?zipcode=${data}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}




export async function getProfile(token) {
  const res = await fetch(`${API_URL}/Users/profile`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return await res.json();
}

export async function GetFamilyDepedentFormattedData(token) {
  const res = await fetch(`${API_URL}/Users/GetFamilyDepedentFormattedData`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return await res.json();
}

export async function GetFamilyDepedent(token) {
  const res = await fetch(`${API_URL}/Users/GetFamilyDepedent`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return await res.json();
}

export async function GetMemberById(data) 
{

 const res = await fetch(`${API_URL}/Users/GetMemberById?memberId=${data}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}

export async function uploadfile(formData) {
  try {
    console.log("axios",formData);
    const response = await axios.post(`${API_URL}/Users/Uploadfile`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    console.log('Upload successful: ' + response.data.fileName);
    setselectedFile(null);
  } catch (error) {
    console.log('Upload failed',error);
  }

}


export async function DeleteMemeber(data) {
    try {

  console.log("API URL:", `${API_URL}/Users/DeleteMember?MemberId=${data}`);
  const res = await fetch(`${API_URL}/Users/DeleteMember?MemberId=${data}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },    
  });
  return await res.json();
  } catch (err) {   
    console.error("Error in DeleteMemeber API:", err);
    return { message: "Error in DeleteMemeber API:" };
  } finally {

  } 
}

export async function UpdatePasswordLinkShow(data) {
    try {

  console.log("API URL:", `${API_URL}/Users/UpdatePasswordLinkShow?userID=${data}`);
  const res = await fetch(`${API_URL}/Users/UpdatePasswordLinkShow?userID=${data}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },    
  });
  return await res.json();
  } catch (err) {   
    console.error("Error in UpdatePasswordLinkShow API:", err);
    return { message: "Error in UpdatePasswordLinkShow API:" };
  } finally {
  }
}

  

 export async function SearchAdmin_UserManagementPage(data) 
{

  const res = await fetch(`${API_URL}/Users/SearchAdmin_UserManagementPage?query=${encodeURIComponent(data)}`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },  
  });
  return await res.json();
}


  export async function SaveUserAsync(data) {
  const res = await fetch(`${API_URL}/Users/SaveUserAsync`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
   const response = await res.json();
   return { status: response.status, message: response.message };
   
}


export async function DeleteUser(data) {
    try {

  console.log("API URL:", `${API_URL}/Users/DeleteUser?UserId=${data}`);
  const res = await fetch(`${API_URL}/Users/DeleteUser?UserId=${data}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },    
  });
  return await res.json();
  } catch (err) {   
    console.error("Error in DeleteUser API:", err);
    return { message: "Error in DeleteUser API:" };
  } finally {

  } 
}