from __future__ import annotations

from dataclasses import dataclass, field
from enum import StrEnum
from typing import TypeAlias


class Status(StrEnum):
    PASS = "PASS"
    FAIL = "FAIL"
    ERROR = "ERROR"


@dataclass(slots=True)
class KeywordResult:
    status: Status = Status.PASS
    output: str = ""


@dataclass(slots=True)
class Statement:
    keyword: str
    library: str
    arguments: list[str] = field(default_factory=list)
    line_number: int = 0
    always_pass: bool = False


@dataclass(slots=True)
class RepeatBlock:
    count: str
    body: list[Runnable]
    always_pass: bool = False


@dataclass(slots=True)
class ForBlock:
    values: list[str]
    body: list[Runnable]
    always_pass: bool = False


@dataclass(slots=True)
class WhileBlock:
    condition: Statement
    body: list[Runnable]
    max_iterations: str = "50"
    while_pass: bool = True
    always_pass: bool = False


@dataclass(slots=True)
class ForEachRowBlock:
    dataset: str
    sheet: str
    table: str
    body: list[Runnable]
    line_number: int = 0
    always_pass: bool = False


@dataclass(slots=True)
class ForSelectedRowBlock:
    dataset: str
    sheet: str
    table: str
    filters: dict[str, tuple[str, list[str]]]
    body: list[Runnable]
    line_number: int = 0
    always_pass: bool = False


@dataclass(slots=True)
class ConditionalBlock:
    condition: Statement
    branches: dict[Status, list[Runnable]] = field(default_factory=dict)
    always_pass: bool = False
    ignored_branches: set[Status] = field(default_factory=set)


Runnable: TypeAlias = (
    Statement
    | RepeatBlock
    | ForBlock
    | WhileBlock
    | ForEachRowBlock
    | ForSelectedRowBlock
    | ConditionalBlock
)


@dataclass(slots=True)
class TestCase:
    name: str
    steps: list[Runnable]
    line_number: int


@dataclass(slots=True)
class TestSuite:
    name: str
    test_cases: list[TestCase]
    variables: dict[str, str] = field(default_factory=dict)
    libraries: dict[str, str] = field(default_factory=dict)
    datasets: dict[str, str] = field(default_factory=dict)
    resources: dict[str, str] = field(default_factory=dict)
    script_folder: str = ""


@dataclass(slots=True)
class StepResult:
    keyword: str
    line_number: int
    status: Status
    output: str = ""


@dataclass(slots=True)
class TestCaseResult:
    name: str
    status: Status
    steps: list[StepResult] = field(default_factory=list)


@dataclass(slots=True)
class ExecutionResult:
    suite_name: str
    status: Status
    test_cases: list[TestCaseResult]

    def to_dict(self) -> dict[str, object]:
        return {
            "suite": self.suite_name,
            "status": self.status.value,
            "test_cases": [
                {
                    "name": test_case.name,
                    "status": test_case.status.value,
                    "steps": [
                        {
                            "keyword": step.keyword,
                            "line": step.line_number,
                            "status": step.status.value,
                            "output": step.output,
                        }
                        for step in test_case.steps
                    ],
                }
                for test_case in self.test_cases
            ],
        }
