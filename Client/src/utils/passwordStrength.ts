import zxcvbn from 'zxcvbn';

export const checkPasswordStrength = (password: string): {
  score: number;
  feedback: string;
  color: string;
} => {
  const result = zxcvbn(password);
  let feedback = '';
  let color = '';

  switch (result.score) {
    case 0:
      feedback = 'Very weak - Easily guessable';
      color = 'text-red-500';
      break;
    case 1:
      feedback = 'Weak - Could be guessed';
      color = 'text-orange-500';
      break;
    case 2:
      feedback = 'Fair - Somewhat secure';
      color = 'text-yellow-500';
      break;
    case 3:
      feedback = 'Good - Difficult to guess';
      color = 'text-blue-500';
      break;
    case 4:
      feedback = 'Strong - Very secure';
      color = 'text-green-500';
      break;
    default:
      feedback = 'Unknown strength';
      color = 'text-gray-500';
  }

  return { score: result.score, feedback, color };
};

export const getSpecificFeedback = (password: string): string => {
  const result = zxcvbn(password);
  
  if (result.feedback.warning) {
    return result.feedback.warning;
  }
  
  if (result.feedback.suggestions.length > 0) {
    return result.feedback.suggestions[0];
  }
  
  return '';
};