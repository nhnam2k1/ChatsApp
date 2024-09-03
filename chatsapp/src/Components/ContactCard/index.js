import { useState } from 'react';

const ContactCard = ({contact, callback}) => {
    const checkRead = contact.unread ? 'unread' : '';
    
    const [isRead, setRead] = useState(checkRead);
    const handleClick = () => {
        setRead('');
        callback(contact);
    };

    return(
        <div data-testid="contact-card"
            className="contact-card" onClick={handleClick}>
            <img src={contact.profilePicture} alt={contact.name} />
            <div className="contact-info">
                <h3 className={isRead}>{contact.name}</h3>
                <p>{contact.lastMessage}</p>
            </div>
        </div>
    );
};

export default ContactCard;