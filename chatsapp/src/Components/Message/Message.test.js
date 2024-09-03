// Message.test.js

import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import Message from './index.js';

const mockMessage = {
  id: 1,
  sender: 'user',
  message: 'Hello',
  isAttachment: true,
};

const mockCallback = jest.fn();

describe('Message', () => {
  test('renders message correctly', () => {
    render(<Message msg={mockMessage} callback={mockCallback} />);
    
    expect(screen.getByText(mockMessage.message)).toBeInTheDocument();
  });

  test('applies correct class based on sender', () => {
    render(<Message msg={mockMessage} callback={mockCallback} />);
    
    const messageContainer = screen.getByText(mockMessage.message).closest('.message-container');
    expect(messageContainer).toHaveClass('user');
  });

  test('renders download button if message is an attachment', () => {
    render(<Message msg={mockMessage} callback={mockCallback} />);
    
    const downloadButton = screen.getByRole('button');
    expect(downloadButton).toBeInTheDocument();
  });

  test('does not render download button if message is not an attachment', () => {
    const messageWithoutAttachment = { ...mockMessage, isAttachment: false };
    render(<Message msg={messageWithoutAttachment} callback={mockCallback} />);
    
    const downloadButton = screen.queryByRole('button');
    expect(downloadButton).not.toBeInTheDocument();
  });

  test('calls callback with correct parameters when download button is clicked', async () => {
    render(<Message msg={mockMessage} callback={mockCallback} />);
    
    const downloadButton = screen.getByRole('button');
    fireEvent.click(downloadButton);
    
    expect(mockCallback).toHaveBeenCalledWith(mockMessage.id, mockMessage.message);
  });
});
