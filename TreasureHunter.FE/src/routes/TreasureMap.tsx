import { Button, TextField, Typography } from "@mui/material";
import { styled } from "@mui/system";
import { useForm } from "react-hook-form";
import * as z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import * as React from "react";
const apiEndpoint = "http://localhost:5172/api";
const mapToMatrixNumber = (input: string) =>
  input
    .trim()
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line)
    .map((line) =>
      line
        .split(" ")
        .map((e) => e.trim())
        .filter((e) => e)
        .map((e) => Number.parseInt(e))
    );
const matrixSchema = z
  .object({
    n: z.coerce.number().min(1).max(500),
    m: z.coerce.number().min(1).max(500),
    p: z.coerce.number().min(1),
    matrix: z.string().min(1),
  })
  .refine((data) => data.p && data.n && data.m && data.p <= data.n * data.m, {
    path: ["p"],
    message: "p need to smaller n x m",
  })
  .refine(
    (data) => {
      const matrix = mapToMatrixNumber(data.matrix);
      return (
        matrix.flatMap((row) => row).every((e) => e) &&
        matrix.length === data.n &&
        matrix.every((row) => row.length === data.m)
      );
    },
    { path: ["matrix"] }
  );

type MatrixForm = {
  p: number;
  m: number;
  n: number;
  matrix: string;
};

export const TreasureMap = () => {
  const {
    register,
    getValues,
    trigger,
    formState: { errors },
  } = useForm<MatrixForm>({
    resolver: zodResolver(matrixSchema),
    defaultValues: {
      m: 0,
      n: 0,
      p: 0,
      matrix: "",
    },
  });
  const [distance, setDistance] = React.useState<string | undefined>();
  const handlerCalculate = async () => {
    const valid = trigger();
    if (!valid) return;
    console.log(valid);
    const form = getValues();
    const body = { p: form.p, matrix: mapToMatrixNumber(form.matrix) };
    const response = await fetch(
      `${apiEndpoint}/treasure-map/calculate-route`,
      {
        method: "POST",
        body: JSON.stringify(body),
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    const distance = await response.text();
    setDistance(distance);
  };
  return (
    <Container>
      <Typography align="left" fontWeight={1000}>
        Tìm kho báu
      </Typography>
      <Typography align="left">
        Đoàn hải tặc tìm thấy một bản đồ kho báu, tuy nhiên để đến được kho báu
        thì phải vượt qua được thử thách. Vùng biển chứa kho báu là một ma trận
        các hòn đảo n hàng m cột, mỗi đảo có một chiếc rương đánh dấu bởi một số
        nguyên dương trong khoảng từ 1 đến p (tạm gọi là số x), và nó sẽ chứa
        chìa khoá cho chiếc rương đánh số x + 1. Và chỉ có chiếc rương được đánh
        số p (và là số lớn nhất) là chứa kho báu. Để đi từ hòn đảo (x1:y1) đến
        đảo (x2:y2) cần một lượng nhiên liệu là √(𝑥1 − 𝑥2) 2 + (𝑦1 − 𝑦2) 2.
      </Typography>
      <Typography align="left">
        Hải tặc đang ở hòn đảo (1:1) - hàng 1 cột 1 và đã có sẵn chìa khoá số 0.
        Với việc cần tiết kiệm nhiên liệu để trở về, hãy tính lượng nhiên liệu
        ít nhất để lấy được kho báu. Biết rằng luôn có đường dẫn đến kho báu
      </Typography>
      <InputSize>
        <TextField
          {...register("p")}
          error={!!errors.p}
          helperText={errors.p?.message}
          type="number"
          label="p (Max value)"
        />
        <TextField
          {...register("m")}
          error={!!errors.m}
          helperText={errors.m?.message}
          type="number"
          label="m (#Column)"
        />
        <TextField
          {...register("n")}
          error={!!errors.n}
          helperText={errors.n?.message}
          type="number"
          label="n (#Row)"
        />
      </InputSize>
      <InputMatrix>
        <TextField
          {...register("matrix")}
          label="Matrix map (divide by space and new line character)"
          multiline
          rows={5}
          error={!!errors.matrix}
          helperText={errors.matrix?.message}
        />
      </InputMatrix>
      <Buttons>
        <Button onClick={handlerCalculate}>Calculate</Button>
        <Button disabled>Save</Button>
      </Buttons>
      {distance !== undefined && (
        <Typography align="left">Calculated distance: {distance}</Typography>
      )}
    </Container>
  );
};

const Container = styled("div")`
  display: flex;
  flex-direction: column;
  gap: 12px;
`;

const InputSize = styled("div")`
  display: flex;
  flex-direction: row;
  gap: 16px;
`;

const InputMatrix = styled("div")`
  display: grid;
`;

const Buttons = styled("div")`
  display: flex;
  flex-direction: row;
`;
