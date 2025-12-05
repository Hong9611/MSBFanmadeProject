# My Sweet Bakery! (모작)

브라우저/PC에서 플레이할 수 있는 캐주얼 제빵 게임 "My Sweet Bakery!"를 모작한 프로젝트입니다.  
원작의 기본 게임 흐름과 UI를 참고하여, 게임 제작 연습 및 포트폴리오용으로 개발했습니다.

> 이 프로젝트는 개인 학습 및 포트폴리오 목적의 비공식 팬 메이드 프로젝트이며,  
> 원작 게임 및 원저작권자와 아무런 관련이 없습니다.
> 사용한 에셋의 저작권 보호를 위해 해당 리포에서는 에셋이 제거되어있습니다.

---

## 원작 정보 (Reference)

- 원작 게임: My Sweet Bakery!
- 플랫폼: [GooglePlay](https://play.google.com/store/apps/details?id=io.supercent.caketopia&hl=ko)
- 개발사: [Supercent](https://corp.supercent.io/)

이 프로젝트는 위 원작 게임의  
- 전반적인 게임 흐름(손님 방문 → 전시 → 판매)  
- 화면 구성 및 UI 흐름  
을 참고하여 구현한 모작입니다.

---

## 주요 기능 (Features)

- 손님이 방문하여 진열대에서 대기
- 제작된 빵을 운반하여 진열대에 진열
- 진열된 빵을 손님이 가지고 계산대 앞에 1열로 서서 기다림
- 계산대에서 손님의 계산을 완료해주면 재화 생성
- 재화로 잠겨있는 구역을 해금

---

## 사용 기술 (Tech Stack)

- Engine/Framework: Unity [2021.3.15f1 → 2021.3.45f2](Unity 버전 보안 이슈)
- Language: C#
- Tools: GitHub, GitHub_Desktop
- DOTween: 재화 획득 연출
- Cinemachine: 카메라 이동 연출
- Queue: 손님 줄세우기
- SerializableDictionary: 테스팅 원활성을 위해 사용

---

## 플레이 방법 (How to Play)

- 화면 하단부 드래그 시 컨트롤러 생성
- 플레이어 캐릭터를 움직여서 점내 기물과 상호 작용

---

## 사용 에셋 (Assets)

- Joystick Pack – Unity Asset Store (Standard Unity Asset Store EULA)
- Serialized Dictionary – Unity Asset Store (Standard Unity Asset Store EULA)
- DOTween (HOTween v2) – Unity Asset Store (Standard Unity Asset Store EULA)
