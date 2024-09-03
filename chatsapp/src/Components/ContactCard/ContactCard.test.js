import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import ContactCard from './index.js';

const mockContact = {
  id: 1,
  name: 'John Doe',
  profilePicture: 'john.jpg',
  lastMessage: 'Hello',
  unread: true,
};

const mockCallback = jest.fn();

describe('ContactCard', () => {
  test('renders contact card correctly', () => {
    render(<ContactCard contact={mockContact} callback={mockCallback} />);
    
    expect(screen.getByAltText(mockContact.name)).toBeInTheDocument();
    expect(screen.getByText(mockContact.name)).toBeInTheDocument();
    expect(screen.getByText(mockContact.lastMessage)).toBeInTheDocument();
  });

  test('applies unread class to contact name if contact is unread', () => {
    render(<ContactCard contact={mockContact} callback={mockCallback} />);
    
    const contactName = screen.getByText(mockContact.name);
    expect(contactName).toHaveClass('unread');
  });

  test('removes unread class and calls callback when contact card is clicked', () => {
    render(<ContactCard contact={mockContact} callback={mockCallback} />);
    
    const contactCard = screen.getByTestId('contact-card');
    const contactName = screen.getByText(mockContact.name);
    
    fireEvent.click(contactCard);
    
    expect(contactName).not.toHaveClass('unread');
    expect(mockCallback).toHaveBeenCalledWith(mockContact);
  });
});
