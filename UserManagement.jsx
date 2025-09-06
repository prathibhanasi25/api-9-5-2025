import Layout from '../../components/Layout';
import { useAuth } from '../../components/authContext';
import { 
  SearchAdmin_UserManagementPage, 
  GetUserRole, 
  SaveUserAsync, 
  DeleteUser
} from '../../services/api';
import React, { useState, useEffect } from "react";
import {
  AppBar,
  Toolbar,
  Typography,
  InputBase,
  Box,
  Container,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Select,
  FormControl,
  InputAdornment,
} from "@mui/material";
import { Add, Edit, Delete, VpnKey } from "@mui/icons-material";
import SearchIcon from '@mui/icons-material/Search';

export default function UserManagement() {
  const { user } = useAuth();
  const [rows, setRows] = useState([]);
  const [searchInput, setSearchInput] = useState("");
  const [open, setOpen] = useState(false);
  const [roles, setRoles] = useState([]);
  const [newUser, setNewUser] = useState({
    firstName: "",
    lastName: "",
    email: "",
    roleId: "",
    status: "Active",
  });
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingUserId, setEditingUserId] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [userToDelete, setUserToDelete] = useState(null);
  const [resetPasswordDialogOpen, setResetPasswordDialogOpen] = useState(false);
  const [ResetPassword, setResetPassword] = useState(null);

  // fetch users when searchInput changes
  useEffect(() => {
    handleSearch();
  }, [searchInput]);

  const handleSearch = async () => {
    try {
      const query = searchInput.trim();
      const response = await SearchAdmin_UserManagementPage(query);
      const users = Array.isArray(response) ? response : [];

      const formattedData = users.map((u) => ({
  id: u.userID,
  firstName: u.firstName || u.FirstName || "",
  lastName: u.lastName || u.LastName || "",
  name: `${u.firstName || u.FirstName || ""} ${u.lastName || u.LastName || ""}`.trim(),
  email: u.email || u.EmailID || "",
  roleId: u.roleId || u.RoleId,
  role: u.roleName || u.RoleName || "",
  lastActive: u.lastLoginDate || u.LastLoginDate || "N/A",
  status: u.isActive || u.IsActive ? "Active" : "Inactive",
}));


      setRows(formattedData);4
    } catch (error) {
      console.error("Search error:", error);
      alert("Search failed. Please try again.");
    }
  };

  // fetch roles once
  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const res = await GetUserRole();
        setRoles(res?.data || res || []);
      } catch (err) {
        console.error("Error fetching roles:", err);
      }
    };
    fetchRoles();
  }, []);

  const handleOpen = () => {
    setIsEditMode(false);
    setEditingUserId(null);
    setNewUser({
      firstName: "",
      lastName: "",
      email: "",
      roleId: "",
      status: "Active",
    });
    setOpen(true);
  };

  const handleClose = () => {
    setIsEditMode(false);
    setEditingUserId(null);
    setOpen(false);
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setNewUser((prev) => ({
      ...prev,
      [name]: name === "roleId" ? Number(value) : value,
    }));
  };

  const handleAddUser = async () => {
    try {
      const payload = {
        UserID: isEditMode ? editingUserId : 0,
        FirstName: newUser.firstName,
        LastName: newUser.lastName,
        EmailID: newUser.email,
        RoleId: Number(newUser.roleId),
        IsActive: newUser.status === "Active",
        AddedBy: user?.firstName || "System",
        ModifiedBy: user?.firstName || "System",
      };

      const response = await SaveUserAsync(payload);
      if (response.status === "Success") {
        alert(response.message);
        handleClose();
        handleSearch();
      } else {
        alert(`Save failed: ${response.message}`);
      }
    } catch (error) {
      console.error("Save error:", error);
      alert("Something went wrong. Please try again.");
    }
  };

  const handleEditUser = (row) => {
     console.log("Editing user row:", row);  
    setIsEditMode(true);
    setEditingUserId(row.id);

    setNewUser({
      firstName: row.firstName || "",
      lastName: row.lastName || "",
      email: row.email || "",
      roleId: row.roleId || "",
      status: row.status,
    });
    setOpen(true);
  };

  const handleDeleteUser = (row) => {
    setUserToDelete(row.id);
    setDeleteDialogOpen(true);
  };

  const confirmDeleteUser = async () => {
    if (!userToDelete) return;
    try {
      const response = await DeleteUser(userToDelete);
      if (response.status === "Success") {
        alert(response.message);
        handleSearch();
      } else {
        alert(`Failed to delete user: ${response.message}`);
      }
    } catch (error) {
      console.error("Error deleting user:", error);
      alert("Something went wrong. Please try again.");
    } finally {
      setDeleteDialogOpen(false);
      setUserToDelete(null);
    }
  };

  const handleResetPassword = (row) => {
    setResetPassword(row);
    setResetPasswordDialogOpen(true);
  };

  const confirmResetPassword = () => {
    alert(`Password reset link would be sent to ${ResetPassword.email}`);
    setResetPasswordDialogOpen(false);
  };

  return (
    <Layout title="User Management">
      <Box>
        <AppBar position="static" sx={{ backgroundColor: "white", color: "black" }}>
          <Toolbar>
            <Typography variant="h5" sx={{ flexGrow: 1, fontWeight: "bold" }}>
              User Management
            </Typography>

            <InputBase
              placeholder="Search…"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              startAdornment={
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              }
              sx={{
                backgroundColor: "#f1f1f1",
                borderRadius: 5,
                paddingLeft: 1,
                marginRight: 2,
                width: "300px",
                border: "1px solid #ccc",
              }}
            />

            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={handleOpen}
              sx={{ backgroundColor: "#1976d2", "&:hover": { backgroundColor: "#1565c0" } }}
            >
              Add User
            </Button>
          </Toolbar>
        </AppBar>

        <Container sx={{ mt: 3 }}>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Role</TableCell>
                  <TableCell>Last Active</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {rows.map((row) => (
                  <TableRow key={row.id}>
                    <TableCell>{row.name}</TableCell>
                    <TableCell>{row.email}</TableCell>
                    <TableCell>{row.role}</TableCell>
                    <TableCell>{row.lastActive}</TableCell>
                    <TableCell>
                      <Chip
                        label={row.status}
                        size="small"
                        sx={{
                          backgroundColor: row.status === "Active" ? "#C8E6C9" : "#FFCDD2",
                          color: row.status === "Active" ? "#2E7D32" : "#C62828",
                          fontWeight: 600,
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <IconButton onClick={() => handleEditUser(row)}  >
                        <Edit />
                      </IconButton>
                      <IconButton onClick={() => handleDeleteUser(row)}  disabled={row.status === "Inactive"}>
                        <Delete />
                      </IconButton>
                      <IconButton onClick={() => handleResetPassword(row)}  disabled={row.status === "Inactive"}>
                        <VpnKey fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Container>

        {/* Add/Edit Dialog */}
        {/* Add/Edit Dialog */}
<Dialog
  open={open}
  onClose={handleClose}
  fullWidth
  maxWidth="xs"
  PaperProps={{
    component: "form", // ✅ enables HTML5 form validation
    onSubmit: (e) => {
      e.preventDefault();
      handleAddUser(); // runs only if inputs pass HTML5 validation
    },
    sx: { borderRadius: 3, boxShadow: 6 },
  }}
>
  <DialogTitle>{isEditMode ? "Edit User" : "Add User"}</DialogTitle>

  <DialogContent
    sx={{
      display: "flex",
      flexDirection: "column",
      gap: 1.5,
    }}
  >
    <TextField
      name="firstName"
      label="First Name"
      value={newUser.firstName}
      onChange={handleInputChange}
      required
      size="small"
    />
    <TextField
      name="lastName"
      label="Last Name"
      value={newUser.lastName}
      onChange={handleInputChange}
      required
      size="small"
    />
    <TextField
      name="email"
      label="Email"
      type="email" // ✅ HTML email validation
      value={newUser.email}
      onChange={handleInputChange}
      required
      size="small"
      InputProps={{ readOnly: isEditMode }}
    />

    <FormControl size="small" required>
      <Select
        name="roleId"
        value={newUser.roleId}
        onChange={handleInputChange}
        displayEmpty
      >
        <MenuItem value="" disabled>Select Role</MenuItem>
        {roles.map((r) => (
          <MenuItem key={r.id} value={r.id}>
            {r.type}
          </MenuItem>
        ))}
      </Select>
    </FormControl>

    <FormControl size="small" required>
      <Select
        name="status"
        value={newUser.status}
        onChange={handleInputChange}
      >
        <MenuItem value="Active">Active</MenuItem>
        <MenuItem value="Inactive">Inactive</MenuItem>
      </Select>
    </FormControl>
  </DialogContent>

  <DialogActions>
    <Button onClick={handleClose}>Cancel</Button>
    <Button variant="contained" type="submit">
      {isEditMode ? "Update" : "Save"}
    </Button>
  </DialogActions>
</Dialog>


        {/* Delete Dialog */}
        <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
          <DialogTitle>Confirm Deletion</DialogTitle>
          <DialogContent>
            Are you sure you want to delete this user? This action cannot be undone.
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
            <Button color="error" variant="contained" onClick={confirmDeleteUser}>
              Delete
            </Button>
          </DialogActions>
        </Dialog>

        {/* Reset Password Dialog */}
        <Dialog open={resetPasswordDialogOpen} onClose={() => setResetPasswordDialogOpen(false)}>
          <DialogTitle>Reset User Password</DialogTitle>
          <DialogContent>
            Are you sure you want to reset the password for <strong>{ResetPassword?.name}</strong>?
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setResetPasswordDialogOpen(false)}>Cancel</Button>
            <Button variant="contained" onClick={confirmResetPassword}>Reset</Button>
          </DialogActions>
        </Dialog>
      </Box>
    </Layout>
  );
}
