-- Admin kullanıcısının şifresini güncelle
-- Şifre: Admin123!
-- Hash: $2a$11$q4GekRA437jGViXGEMUInOXsoLMRC80R.aKYMdJVuqsXe0s2Lg.aO

UPDATE Users 
SET PasswordHash = '$2a$11$q4GekRA437jGViXGEMUInOXsoLMRC80R.aKYMdJVuqsXe0s2Lg.aO'
WHERE Email = 'admin@insurance.com';

-- Güncelleme sonrası kontrol
SELECT Id, Name, Email, Role, PasswordHash 
FROM Users 
WHERE Email = 'admin@insurance.com';
