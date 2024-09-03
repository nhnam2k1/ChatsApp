import {useState, useEffect} from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import PageLoader from "../Components/page-loader";
import UpdateProfileForm from './UpdateForm.js';

const domain = process.env.REACT_APP_AUTH0_DOMAIN;
const audience = `https://${domain}/api/v2/`;

const Profile = () => {
    const { user, isLoading, getAccessTokenSilently } = useAuth0();
    const [metadata, setMetadata] = useState();

    useEffect(() => {
        const url = `${audience}users/${user.sub}`;
        getAccessTokenSilently()
        .then(token => fetch(`${url}?fields=user_metadata&include_fields=true`, {
            method: "GET",
            cache: "no-cache",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`,
            }
        }))
        .then(response => response.json())
        .then(data => setMetadata(data.user_metadata))
        .catch(err => console.error(toString(err)));

    }, [getAccessTokenSilently, user])

    if (isLoading) return <PageLoader/>;

    return (
        <div style={pageStyles}>
            <div style={avatarAreas}>
                <img src={user.picture} alt={user.name} style={avatar}/>
                <div>
                    <h2>{user.email}</h2>
                    <h3>{user.email_verified ? "Email is verified" 
                        : "Email is not verified"}</h3>
                    <p>Name: {metadata?.name}</p>
                    <p>Birthday: {metadata?.birthday}</p>
                    <p>Phone: {metadata?.phone}</p>
                </div>
            </div>
            
            <UpdateProfileForm/>
        </div>
    );
};

const pageStyles = {
    margin: "15px",
    display: "flex",
    flexDirection: "row",
    justifyContent: "flex-start"
};

const avatar = {
    borderRadius: "15px",
    width: "128px",
    height: "128px",
    marginRight: "15px"
};

const avatarAreas = {
    display: "flex",
    flexDirection: "row",
    width: '600px'
};

export default Profile;